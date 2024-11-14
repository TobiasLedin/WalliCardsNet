using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/google-callback")]
    [ApiController]
    public class GoogleCallbackController : ControllerBase
    {
        private readonly ILogger<GoogleCallbackController> _logger;
        private readonly IGooglePassRepo _googlePassRepo;

        public GoogleCallbackController(ILogger<GoogleCallbackController> logger, IGooglePassRepo googlePassRepo)
        {
            _logger = logger;
            _googlePassRepo = googlePassRepo;
        }


        /// <summary>
        /// Handle end-user Add pass / Remove pass.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GoogleWalletCallback([FromBody] GoogleWalletCallback callback)
        {

            if (callback.IsExpired())
            {
                _logger.LogWarning("Received expired callback for object {ObjectId}", callback.ObjectId);
                return BadRequest("Callback expired");
            }

            if (callback.IsSaveEvent())
            {
                _logger.LogInformation("Processing save event for object {ObjectId}", callback.ObjectId);

                var googlePass = await _googlePassRepo.GetByIdAsync(callback.ObjectId);
                if (googlePass != null)
                {
                    googlePass.PassStatus = Enums.PassStatus.Saved;

                    await _googlePassRepo.UpdateAsync(googlePass);

                    _logger.LogInformation($"Google Pass {googlePass.ObjectId} saved to wallet");
                }
            }
            else if (callback.IsDeleteEvent())
            {
                _logger.LogInformation("Processing delete event for object {ObjectId}", callback.ObjectId);

                var googlePass = await _googlePassRepo.GetByIdAsync(callback.ObjectId);
                if (googlePass != null)
                {
                    googlePass.PassStatus = Enums.PassStatus.Deleted;

                    await _googlePassRepo.UpdateAsync(googlePass);

                    _logger.LogInformation($"Google Pass {googlePass.ObjectId} deleted from wallet");
                }
            }

            return Ok();
        }
    }
}
