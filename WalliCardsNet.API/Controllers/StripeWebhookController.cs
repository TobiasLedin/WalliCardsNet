using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/payment-webhook")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> Handle()
        {

            return Ok();
        }

    }
}
