using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using WalliCardsNet.API.Models;
using WalliCardsNet.API.Services;
using WalliCardsNet.ClassLibrary.Login;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGoogleService _googleService;

        public AuthenticationController(IAuthService authService, UserManager<ApplicationUser> userManager, IGoogleService googleService)
        {
            _authService = authService;
            _userManager = userManager;
            _googleService = googleService;
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

        [HttpPost("link/google")]
        public async Task<IActionResult> LinkGoogleAccountAsync([FromBody] string code)
        {
            try
            {
                var tokenData = await _googleService.ExchangeCodeForTokensAsync(code, "https://localhost:7102/auth/google/link/");
                var idToken = tokenData["id_token"].ToString();
                var (googleUserId, googleEmail) = _googleService.DecodeIdToken(idToken);

                var user = await _userManager.FindByEmailAsync(googleEmail);
                if (user == null)
                {
                    return Unauthorized("Account does not exist.");
                }

                var success = await _googleService.LinkGoogleAccountAsync(user, googleUserId);
                if (success)
                {
                    return Ok("Google account linked successfully.");
                }
                return BadRequest("Error linking Google account.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("google")]
        public async Task<IActionResult> SignInGoogleAccountAsync([FromBody] string code)
        {
            try
            {
                var loginResponse = await _authService.LoginWithGoogleAsync(code);

                if (!loginResponse.Success)
                {
                    return Unauthorized(loginResponse.Details);
                }

                return Ok(loginResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // TEST ENDPOINT FOR CREATING USERS
        [HttpPost]
        [Route("register-employee")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterEmployee(string userName, string email, string password)
        {
            var businessIdClaim = User.FindFirst("business-id");
            if (businessIdClaim == null)
            {
                return Unauthorized();
            }

            Guid businessId = Guid.Parse(businessIdClaim.Value);
            var result = await _authService.CreateUserAccountAsync(businessId, Constants.Roles.Employee, userName, email);

            if (result.Success)
            {
                return Created();
            }

            return BadRequest(result.Details);

        }

    }
}
