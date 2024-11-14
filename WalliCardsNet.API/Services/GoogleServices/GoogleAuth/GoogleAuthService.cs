using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Services.GoogleServices.GoogleAuth
{
    public class GoogleAuthService : IGoogleAuth
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(UserManager<ApplicationUser> userManager, ILogger<GoogleAuthService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>> ExchangeCodeForTokensAsync(string code, string redirectUri)
        {
            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            var httpClient = new HttpClient();
            var requestData = new Dictionary<string, string>
            {
                {"code", code },
                {"client_id", Environment.GetEnvironmentVariable("GOOGLE-CLIENT-ID") },
                {"client_secret", Environment.GetEnvironmentVariable("GOOGLE-CLIENT-SECRET") },
                {"redirect_uri", redirectUri },
                {"grant_type", "authorization_code" }
            };

            var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(requestData));
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error exchanging code for tokens");
            }

            return JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
        }
        public (string googleUserId, string googleEmail) DecodeIdToken(string idToken)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(idToken);
            var googleUserId = jwtToken.Claims.First(claim => claim.Type == "sub").Value;
            var googleEmail = jwtToken.Claims.First(claim => claim.Type == "email").Value;
            return (googleUserId, googleEmail);
        }
        public async Task<bool> LinkGoogleAccountAsync(ApplicationUser user, string googleUserId)
        {
            var loginInfo = new UserLoginInfo("Google", googleUserId, "Google");
            var result = await _userManager.AddLoginAsync(user, loginInfo);
            return result.Succeeded;
        }
    }
}
