using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Data.Interfaces;

namespace WalliCardsNet.API.Controllers
{
    [Route("/api/{controller}")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly IBusiness _businessRepo;
        public StripeController(IBusiness businessRepo)
        {
            _businessRepo = businessRepo;
        }


    }
}
