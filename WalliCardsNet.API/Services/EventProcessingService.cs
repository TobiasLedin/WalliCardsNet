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
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            switch (paymentEvent.PaymentServiceProvider)
            {
                case PaymentServiceProviders.Stripe:
                    await ProcessStripeEventAsync(userManager, businessRepository, paymentEvent);
                    break;

                case PaymentServiceProviders.LemonSqueezy:
                    await ProcessLemonSqueezyEventAsync(userManager, businessRepository, paymentEvent);
                    break;

                default:
                    _logger.LogError("Invalid payment service provider");
                    break;
            };
        }

        private async Task ProcessStripeEventAsync(UserManager<ApplicationUser> userManager, IBusiness businessRepo, PaymentEvent paymentEvent)
        {
            Business? business = null;
            ApplicationUser? user = null;
            Stripe.Subscription? subscription = null;
            Stripe.Invoice? invoice = null;
            string? subscriptionType = null;

            switch (paymentEvent.EventType)
            {
                case Events.InvoicePaid:

                    invoice = paymentEvent.EventData as Stripe.Invoice;

                    if (invoice != null)
                    {
                        // Update existing business subscription end date and return.
                        if (await businessRepo.BusinessWithPspIdExists(invoice.CustomerId))
                        {
                            business = await businessRepo.GetByIdAsync(invoice.CustomerId);

                            business.SubscriptionEndDate = invoice.Subscription.CurrentPeriodEnd;

                            if (DateTime.UtcNow < invoice.Subscription.CurrentPeriodEnd)
                            {
                                business.SubscriptionActive = true;
                            }

                            // TRIGGER EMAIL RECEIPT

                            //invoice.HostedInvoiceUrl
                            //invoice.InvoicePdf

                            return;
                        }

                        // Handle new business subscription (creation of Business and ApplicationUser entities).
                        foreach (var lineItem in invoice.Lines)
                        {
                            if (lineItem.Plan.Active)
                            {
                                subscriptionType = lineItem.Plan.Interval;
                            }
                        }

                        if (invoice.CustomerId != null && subscriptionType != null)
                        {
                            business = new Business
                            {
                                PspId = invoice.CustomerId,
                                SubscriptionType = subscriptionType

                            };

                            user = new ApplicationUser
                            {
                                UserName = invoice.CustomerName,
                                NormalizedUserName = invoice.CustomerName.ToUpper(),
                                Email = invoice.CustomerEmail,
                                NormalizedEmail = invoice.CustomerEmail.ToUpper(),
                                EmailConfirmed = true,
                                Business = business
                            };

                            try
                            {
                                await businessRepo.AddAsync(business);
                                await userManager.CreateAsync(user);
                                await userManager.AddToRoleAsync(user, Constants.Roles.Manager);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error during Business or ApplicationUser creation");
                            }
                        }
                    }
                    break;

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
                                business.SubscriptionActive = true;
                            }

                            await businessRepo.UpdateAsync(business);
                        }
                    }
                    break;

                case Events.CustomerSubscriptionDeleted:

                    subscription = paymentEvent.EventData as Stripe.Subscription;

                    if (subscription != null)
                    {
                        business = await businessRepo.GetByIdAsync(subscription.CustomerId);

                        business.SubscriptionType = "cancelled";

                        try
                        {
                            await businessRepo.UpdateAsync(business);
                        }
                        catch (Exception ex)
                        {
                        }
                          
                    }
                    break;

                case Events.CustomerCreated: // Handled in checkout session completed
                case Events.ChargeFailed:
                case Events.InvoicePaymentFailed:
                case Events.CustomerUpdated:
                case Events.CustomerDeleted:
                case Events.CustomerSubscriptionCreated:
                case Events.InvoicePaymentSucceeded:

                    _logger.LogInformation("Method to manage Stripe event type not yet implemented: {}", paymentEvent.EventType);
                    break;

                default:

                    _logger.LogInformation("No method to manage Stripe event type: {}", paymentEvent.EventType);
                    break;
            }

        }

        private async Task ProcessLemonSqueezyEventAsync(UserManager<ApplicationUser> userManager, IBusiness businessRepo, PaymentEvent paymentEvent)
        {
            throw new NotImplementedException();
        }

    }
}


