using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Threading.Channels;
using WalliCardsNet.API.Constants;
using WalliCardsNet.API.Models;
using WalliCardsNet.API.Services;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/payment-webhook")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly string _stripeWebhookSecret;
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly Channel<PaymentEvent> _eventQueue;
        private readonly EventProcessingService _processingService;

        public StripeWebhookController(ILogger<StripeWebhookController> logger, Channel<PaymentEvent> eventQueue)
        {
            _logger = logger;
            _eventQueue = eventQueue;
            _stripeWebhookSecret = Environment.GetEnvironmentVariable("STRIPE-WEBHOOK-SECRET")
                ?? throw new NullReferenceException("Environment variable cannot be read");
        }

        [HttpPost]
        public async Task<IActionResult> HandleEvent()
        {
            var stripeSignature = Request.Headers["Stripe-Signature"];
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _stripeWebhookSecret);

                var paymentEvent = new PaymentEvent
                {
                    EventId = stripeEvent.Id,
                    PaymentServiceProvider = PaymentServiceProviders.Stripe,
                    EventType = stripeEvent.Type,
                    EventData = stripeEvent.Data.Object,
                    Received = DateTime.UtcNow
                };

                await _eventQueue.Writer.WriteAsync(paymentEvent);
                
                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook signature verification failed");
                return BadRequest();
            }
        }
    }
}
