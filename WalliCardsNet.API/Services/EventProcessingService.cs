using System.Collections.Concurrent;
using WalliCardsNet.API.Data;
using WalliCardsNet.API.Models;
using Stripe;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using WalliCardsNet.API.Data.Repositories;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Constants;
using System.Threading.Channels;
using System.Configuration;
using System.Reflection.Metadata.Ecma335;
using SendGrid.Helpers.Mail;
using WalliCardsNet.ClassLibrary.Register;

namespace WalliCardsNet.API.Services
{
    public class EventProcessingService : BackgroundService
    {

        private readonly ProcessedEventStorage _eventStorage;
        private readonly ILogger<EventProcessingService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Channel<PaymentEvent> _eventQueue;

        public EventProcessingService(ProcessedEventStorage eventStorage, ILogger<EventProcessingService> logger, IServiceProvider serviceProvider, Channel<PaymentEvent> eventQueue)
        {
            _eventStorage = eventStorage;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _eventQueue = eventQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var paymentEvent in _eventQueue.Reader.ReadAllAsync())
            {
                try
                {
                    await HandlePaymentEventAsync(paymentEvent);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while processing PaymentEvent");
                }
            }
        }

        private async Task HandlePaymentEventAsync(PaymentEvent paymentEvent)
        {
            if (_eventStorage.EventExists(paymentEvent.EventId))
                return;

            try
            {
                await ProcessEventAsync(paymentEvent);
                _eventStorage.MarkAsProcessed(paymentEvent.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment event {EventId}. Data: {EventData}",
                    paymentEvent.EventId, paymentEvent.EventData is string jsonData ? jsonData : JsonSerializer.Serialize(paymentEvent.EventData));
            }
        }

        private async Task ProcessEventAsync(PaymentEvent paymentEvent)
        {
            using var scope = _serviceProvider.CreateScope();
            var businessRepository = scope.ServiceProvider.GetRequiredService<IBusiness>();
            //var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var authService = scope.ServiceProvider.GetService<IAuthService>();
            var mailService = scope.ServiceProvider.GetService<IMailService>();

            switch (paymentEvent.PaymentServiceProvider)
            {
                case PaymentServiceProviders.Stripe:
                    await ProcessStripeEventAsync(authService, businessRepository, paymentEvent, mailService);
                    break;

                case PaymentServiceProviders.LemonSqueezy:
                    await ProcessLemonSqueezyEventAsync(authService, businessRepository, paymentEvent, mailService);
                    break;

                default:
                    _logger.LogError("Invalid payment service provider");
                    break;
            };
        }

        #region Stripe event processing
        private async Task ProcessStripeEventAsync(IAuthService authService, IBusiness businessRepo, PaymentEvent paymentEvent, IMailService mailService)
        {
            Business? business = null;
            Stripe.Subscription? subscription = null;
            Stripe.Invoice? invoice = null;
            string? subscriptionType = null;
            DateTime? subscriptionEnd = null;
            EmailAddress email = null;

            switch (paymentEvent.EventType)
            {
                // Manage Stripe event Invoice.Paid
                case Events.InvoicePaid:

                    invoice = paymentEvent.EventData as Stripe.Invoice;

                    if (invoice != null)
                    {
                        // Handle new business subscription (creation of Business and ApplicationUser entities).
                        foreach (var lineItem in invoice.Lines)
                        {
                            if (lineItem.Plan.Active)   //TODO: Check whether to filter for Active or something else in Lines.
                            {
                                subscriptionType = lineItem.Plan.Interval;
                                subscriptionEnd = lineItem.Period.End;
                            }
                        }

                        // Update existing business SubscriptionEndDate and returns without creating Business and ApplicationUser.
                        if (await businessRepo.BusinessWithPspIdExists(invoice.CustomerId))
                        {
                            business = await businessRepo.GetByIdAsync(invoice.CustomerId);

                            business.SubscriptionEndDate = subscriptionEnd;

                            if (DateTime.UtcNow < subscriptionEnd)
                            {
                                business.SubscriptionStatus = Status.Active;

                                await businessRepo.UpdateAsync(business);
                            }

                            return;
                        }

                        if (invoice.CustomerId != null && subscriptionType != null)
                        {
                            business = new Business
                            {
                                PspId = invoice.CustomerId,
                                SubscriptionType = subscriptionType,
                                SubscriptionStatus = Status.Active,
                                SubscriptionEndDate = subscriptionEnd
                            };

                            //user = new ApplicationUser
                            //{
                            //    UserName = invoice.CustomerName,
                            //    NormalizedUserName = invoice.CustomerName.ToUpper(),
                            //    Email = invoice.CustomerEmail,
                            //    NormalizedEmail = invoice.CustomerEmail.ToUpper(),
                            //    EmailConfirmed = true,
                            //    Business = business
                            //};

                            email = new EmailAddress
                            {
                                Email = invoice.CustomerEmail
                            };

                            try
                            {
                                await businessRepo.AddAsync(business);
                                var registerResult = await authService.CreateUserAccountAsync(business.Id, Constants.Roles.Manager, invoice.CustomerName, invoice.CustomerEmail);

                                if (registerResult.Success && registerResult.UserId != null)
                                {
                                    try
                                    {
                                        await mailService.SendActivationLinkAsync(email, registerResult.UserId);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error while sending an account activation email");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error during Business or ApplicationUser creation");
                            }
                        }
                    }
                    break;

                // Manage Stripe event Invoice.PaymentFailed
                case Events.InvoicePaymentFailed:

                    invoice = paymentEvent.EventData as Stripe.Invoice;

                    if (invoice != null && await businessRepo.BusinessWithPspIdExists(invoice.CustomerId))
                    {
                        business = await businessRepo.GetByIdAsync(invoice.CustomerId);

                        business.SubscriptionStatus = Status.PaymentFailed;

                        await businessRepo.UpdateAsync(business);
                    }
                    break;

                // Manage Stripe event Customer.SubscriptionUpdated
                case Events.CustomerSubscriptionUpdated:

                    subscription = paymentEvent.EventData as Stripe.Subscription;

                    if (subscription != null)
                    {
                        business = await businessRepo.GetByIdAsync(subscription.CustomerId);

                        if (business == null)
                            return;

                        foreach (var lineItem in subscription.Items)
                        {
                            if (lineItem.Plan.Active)
                            {
                                subscriptionType = lineItem.Plan.Interval;
                            }
                        }

                        if (subscriptionType != null)
                        {
                            business.SubscriptionType = subscriptionType;
                            business.SubscriptionEndDate = subscription.CurrentPeriodEnd;

                            if (DateTime.UtcNow < subscription.CurrentPeriodEnd)
                            {
                                business.SubscriptionStatus = Status.Active;
                            }

                            await businessRepo.UpdateAsync(business);
                        }
                    }
                    break;

                // Manage Stripe event Customer.SubscriptionDeleted
                case Events.CustomerSubscriptionDeleted:

                    subscription = paymentEvent.EventData as Stripe.Subscription;

                    if (subscription != null)
                    {
                        business = await businessRepo.GetByIdAsync(subscription.CustomerId);

                        business.SubscriptionStatus = Status.Cancelled;

                        await businessRepo.UpdateAsync(business);
                    }
                    break;

                // Unmanaged Stripe events. Logged for information and testing purposes.
                default:

                    _logger.LogInformation("No method to manage Stripe event type: {}", paymentEvent.EventType);
                    break;
            }
        }
        #endregion

        private async Task ProcessLemonSqueezyEventAsync(IAuthService authService, IBusiness businessRepo, PaymentEvent paymentEvent, IMailService mailService)
        {
            throw new NotImplementedException();
        }

    }
}


