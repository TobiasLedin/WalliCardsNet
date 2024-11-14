using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Models;
using WalliCardsNet.API.Services;
using WalliCardsNet.ClassLibrary.Login;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGoogleService _googleService;

        public AuthenticationController(IConfiguration config, IAuthService authService, ITokenService tokenService, UserManager<ApplicationUser> userManager, IGoogleService googleService)
        {
            _config = config;
            _authService = authService;
            _tokenService = tokenService;
            _userManager = userManager;
            _googleService = googleService;
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var authResult = await _authService.AuthenticateUserAsync(login.Email, login.Password);

            if (authResult.Success)
            {
                var claims = await _tokenService.GenerateClaimsAsync(authResult.User!, authResult.Business!);
                var accessToken = _tokenService.GenerateAccessToken(claims);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(Guid.Parse(authResult.User!.Id));

                if (accessToken == null || refreshToken == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                Response.Cookies.Append("refreshToken", refreshToken, GetAuthCookieOptions());

                return Ok(new LoginResponse(accessToken, authResult.Details));
            }

            return Unauthorized(new LoginResponse(null, authResult.Details));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogoutUserAsync()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await _tokenService.DeleteRefreshTokenAsync(refreshToken);
            }

            return Ok();
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshAuthTokensAsync([FromBody] string accessToken)
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken) && !string.IsNullOrEmpty(accessToken))
            {
                var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
                var isValid = await _tokenService.RefreshTokenValidationAsync(refreshToken);

                if (principal == null || !isValid)
                {
                    return BadRequest("Invalid token");
                }

                try
                {
                    var userIdClaim = principal.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").ToString();

                    Guid userId = Guid.Parse(userIdClaim.Split(':').Last().Trim());

                    var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
                    var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(userId);
                    await _tokenService.DeleteRefreshTokenAsync(refreshToken);

                    Response.Cookies.Append("refreshToken", newRefreshToken, GetAuthCookieOptions());

                    return Ok(new LoginResponse(newAccessToken, "Successfully renewed tokens"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: {0}", ex);
                }  
            }

            return Unauthorized(new LoginResponse(null, "Failed to renewed tokens"));
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
                var loginResult = await _authService.LoginWithGoogleAsync(code);

                if (!loginResult.Success)
                {
                    return Unauthorized(new LoginResponse(null, loginResult.Details));
                }

                return Ok(new LoginResponse(loginResult.AccessToken, loginResult.Details));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        private CookieOptions GetAuthCookieOptions()
        {
            var expiry = _config.GetValue<double>("JwtSettings:RefreshTokenExpireHours");

            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(expiry)
            };

            return cookieOptions;
        }
    }
}
