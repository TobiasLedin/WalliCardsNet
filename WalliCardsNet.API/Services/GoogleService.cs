using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Walletobjects.v1;
using Google.Apis.Walletobjects.v1.Data;
using WalliCardsNet.API.Models;
using WalliCardsNet.API.Builders;
using WalliCardsNet.ClassLibrary.BusinessProfile;

namespace WalliCardsNet.API.Services
{
    public class GoogleService : IGoogleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleService> _logger;
        private WalletobjectsService? _walletService;
        private string? _issuerId;

        public GoogleService(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<GoogleService> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        #region Google Auth
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
        #endregion

        #region Google Pass

        public void GoogleApiAuthentication()
        {
            _issuerId = Environment.GetEnvironmentVariable("GOOGLE-WALLET-ISSUER-ID") ?? throw new NullReferenceException();
            var keyFilePath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS") ?? throw new NullReferenceException();
            var fullPath = Path.GetFullPath(keyFilePath);

            var credentials = (ServiceAccountCredential)GoogleCredential
                .FromFile(keyFilePath)
                .CreateScoped(new List<string>
                {
                    WalletobjectsService.ScopeConstants.WalletObjectIssuer
                })
                .UnderlyingCredential;

            _walletService = new WalletobjectsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials
            });
        }

        public async Task<string> CreateGenericClassAsync(string classSuffix)
        {
            // Authenticate and initialize WalletService
            if (_walletService == null)
            {
                GoogleApiAuthentication();
            }

            string classId = $"{_issuerId}.{classSuffix}";

            _logger.LogInformation(classId);

            // Check for exising GenericClasses with supplied issuerId and classSuffix
            using (Stream responseStream = await _walletService!.Genericclass
                .Get(classId)
                .ExecuteAsStreamAsync())

            using (StreamReader responseReader = new StreamReader(responseStream))
            {
                string responseJson = await responseReader.ReadToEndAsync();
                using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                {
                    JsonElement rootElement = jsonResponse.RootElement;
                    if (!rootElement.TryGetProperty("error", out JsonElement errorElement))
                    {
                        _logger.LogInformation($"Class {classId} already exists!");
                        return classId;
                    }
                    else if (errorElement.GetProperty("code").GetInt32() != 404)
                    {
                        _logger.LogInformation(rootElement.ToString());
                        return classId;
                    }
                }
            }

            // Create new GenericClass
            var newClass = new GenericClass
            {
                Id = classId
            };

            using (Stream responseStream = await _walletService.Genericclass
                .Insert(newClass)
                .ExecuteAsStreamAsync())

            using (StreamReader responseReader = new StreamReader(responseStream))
            {
                string responseJson = await responseReader.ReadToEndAsync();
                using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                {
                    _logger.LogInformation($"Class {classId} successfully created");
                }
            }

            return classId;
        }

        public async Task<string> CreateGenericObjectAsync(GooglePassTemplateDTO dto, string classSuffix, string objectSuffix) // ClassSuffix: BusinessId, ObjectSuffix: CustomerId
        {
            // Authenticate and initialize WalletService
            if (_walletService == null)
            {
                GoogleApiAuthentication();
            }

            string classId = $"{_issuerId}.{classSuffix}";
            string objectId = $"{_issuerId}.{objectSuffix}";

            _logger.LogInformation(classId, objectId);

            // Check for exising GenericClasses with supplied issuerId and classSuffix
            using (Stream responseStream = await _walletService!.Genericobject
                .Get($"{_issuerId}.{objectSuffix}")
                .ExecuteAsStreamAsync())

            using (StreamReader responseReader = new StreamReader(responseStream))
            {
                string responseJson = await responseReader.ReadToEndAsync();
                using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                {
                    JsonElement rootElement = jsonResponse.RootElement;
                    if (!rootElement.TryGetProperty("error", out JsonElement errorElement))
                    {
                        _logger.LogInformation($"Object {objectId} already exists!");
                        return objectId;
                    }
                    else if (errorElement.GetProperty("code").GetInt32() != 404)
                    {
                        _logger.LogInformation(rootElement.ToString());
                        return objectId;
                    }
                }
            }

            using (Stream responseStream = await _walletService.Genericobject
                .Insert( new GooglePassBuilder().CreateObjectFromDTO(dto)) // Calls PassBuilder to create GenericObject from GooglePassTemplateDTO.
                .ExecuteAsStreamAsync())

            using (StreamReader responseReader = new StreamReader(responseStream))
            {
                string responseJson = await responseReader.ReadToEndAsync();
                using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                {
                    _logger.LogInformation($"Object {objectId} successfully created");
                }
            }

            return objectId;
        }

        #endregion
    }
}
