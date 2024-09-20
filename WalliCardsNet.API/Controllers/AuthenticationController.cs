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
            var result = await _authService.LoginAsync(login.Email, login.Password);

            if (result.Success)
            {
                return Ok(result);
            }

            return Unauthorized(result.Details);
        }


        //[HttpPost]
        //[Route("register-employee")]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> RegisterEmployee(RegisterRequestDTO register)
        //{
        //    var result = await _authService.RegisterEmployeeAsync(register.UserName, register.Email, register.);

        //    if (result.Success)
        //    {
        //        return Created();
        //    }

        //    return BadRequest(result.Details);

        //}


        // TEST ENDPOINT FOR CREATING USERS
        [HttpPost]
        [Route("register-employee")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterEmployee(string userName, string email, string password)
        {
            var result = await _authService.RegisterEmployeeAsync(userName, email, password);

            if (result.Success)
            {
                return Created();
            }

            return BadRequest(result.Details);

        }

    }
}
