using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/google-callback")]
    [ApiController]
    public class GoogleCallbackController : ControllerBase
    {
        private readonly ILogger<GoogleCallbackController> _logger;

        public GoogleCallbackController(ILogger<GoogleCallbackController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult GoogleWalletCallback([FromBody] GoogleWalletCallback callback)
        {
            try
            {
                // Check if the callback has expired
                if (callback.IsExpired())
                {
                    _logger.LogWarning("Received expired callback for object {ObjectId}", callback.ObjectId);
                    return BadRequest("Callback expired");
                }

                // Example callback JSON:
                // {
                //   "classId": "3388000000022321761.movie_ticket",
                //   "objectId": "3388000000022321761.movie_ticket_object_1",
                //   "expTimeMillis": 1699209600000,
                //   "eventType": "save",
                //   "nonce": "abc123xyz789"
                // }

                var (issuerId, classId) = callback.ParseClassId();
                var (_, objectId) = callback.ParseObjectId();

                if (callback.IsSaveEvent())
                {
                    // Handle save event
                    _logger.LogInformation("Processing save event for object {ObjectId}", objectId);
                    // Add your save logic here
                }
                else if (callback.IsDeleteEvent())
                {
                    // Handle delete event
                    _logger.LogInformation("Processing delete event for object {ObjectId}", objectId);
                    // Add your delete logic here
                }
                else
                {
                    _logger.LogWarning("Unknown event type {EventType}", callback.EventType);
                    return BadRequest("Unknown event type");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Google Wallet callback");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
