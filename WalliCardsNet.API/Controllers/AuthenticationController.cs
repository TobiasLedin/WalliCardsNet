using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Services;
using WalliCardsNet.ClassLibrary;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {

        private readonly IAuthService _authService;

        public AuthenticationController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginRequestDTO login)
        {
            var result = await _authService.Login(login.Email, login.Password);

            if (result.LoginSuccess)
            {
                return Ok(new LoginResultDTO(true, result.Token));
            }


            return Unauthorized(result.Details);
        }


        // Register TEST -  !!!

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(string name, string email, string password)
        {
            var result = await _authService.Register(name, email, password);

            if (result.RegisterSuccess)
            {
                return Created();
            }

            return BadRequest(result.Details);

        }

    }
}
