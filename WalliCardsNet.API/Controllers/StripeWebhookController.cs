using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/payment-webhook")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly string _stripeWebhookSecret;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(ILogger<StripeWebhookController> logger)
        {
            _stripeWebhookSecret = Environment.GetEnvironmentVariable("STRIPE-WEBHOOK-SECRET")
                ?? throw new NullReferenceException("Environment variable cannot be read");
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleEvent()
        {
            var stripeSignature = Request.Headers["Stripe-Signature"];
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _stripeWebhookSecret);

                _logger.LogInformation("Stripe event received and verified: {Type}", stripeEvent.Type);

                /// Hantera events direkt i controllern?
                //stripeEvent.Type switch
                //{
                //    Events.CheckoutSessionCompleted => do shit
                //    Events.CustomerSubscriptionDeleted => do shit
                //    Events.CustomerSubscriptionUpdated => do shit
                //    _ => _logger.LogInformation("Unhandled event type: {0}", stripeEvent.Type)
                //};

                /// Kör en BackgroundService instans (verkar vara ett bra alternativ och kanske intressant att utforska)?

                /// Kör asynkront jobb utan att invänta svar (en del drawbacks kopplade till detta)?
                //Task.Run(() => HandleEventAsync(stripeEvent));

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook signature verification failed");
                return BadRequest();
            }

            
        }

        private async Task HandleEventAsync(Event stripeEvent)
        {
            throw new NotImplementedException();
        }
    }
}
