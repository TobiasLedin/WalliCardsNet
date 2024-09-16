using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationUser> _roleManager;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationUser> roleManager)
        {
            
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody]LoginDTO login)
        {
            
            return Ok();
        }

    }
}
