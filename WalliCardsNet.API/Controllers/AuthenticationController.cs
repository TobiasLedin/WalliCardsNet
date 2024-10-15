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

        public AuthenticationController(IAuthService authService, UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
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
        public async Task<IActionResult> GoogleAuth([FromBody] string code)
        {
            var tokenEndpoint = "https://oauth2.googleapis.com/token";

            var httpClient = new HttpClient();
            var requestData = new Dictionary<string, string>
            {
                {"code", code },
                {"client_id", Environment.GetEnvironmentVariable("GOOGLE-CLIENT-ID") },
                {"client_secret", Environment.GetEnvironmentVariable("GOOGLE-CLIENT-SECRET") },
                {"redirect_uri", "https://localhost:7102/auth/google/" },
                {"grant_type", "authorization_code" }
            };

            var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(requestData));
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Error exchanging code for tokens");
            }

            Console.WriteLine(responseContent);
            var tokenData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
            var accessToken = tokenData["access_token"];
            var idToken = tokenData["id_token"].ToString();
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(idToken);
            var googleUserId = jwtToken.Claims.First(claim => claim.Type == "sub").Value;
            var googleEmail = jwtToken.Claims.First(claim => claim.Type == "email").Value;

            var user = await _userManager.FindByEmailAsync(googleEmail);

            if (user == null)
            {
                return Unauthorized("Account does not exist.");
            }

            var loginInfo = new UserLoginInfo("Google", googleUserId, "Google");
            var result = await _userManager.AddLoginAsync(user, loginInfo);

            if (result.Succeeded)
            {
                return Ok("Google account linked successfully.");
            }

            return BadRequest("Error linking Google account.");
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
