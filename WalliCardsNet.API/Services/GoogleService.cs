using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Walletobjects.v1;
using Google.Apis.Walletobjects.v1.Data;
using WalliCardsNet.API.Models;

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

        public async Task<string> CreateGenericObjectAsync(string classSuffix, string objectSuffix) // ObjectSuffix => CustomerId
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

            // Create new GenericObject
            var newObject = new GenericObject
            {
                Id = objectId,
                ClassId = classId,
                State = "ACTIVE",

                HeroImage = new Image
                {
                    SourceUri = new ImageUri
                    {
                        Uri = "https://farm4.staticflickr.com/3723/11177041115_6e6a3b6f49_o.jpg"
                    },
                    ContentDescription = new LocalizedString
                    {
                        DefaultValue = new TranslatedString
                        {
                            Language = "en-US",
                            Value = "Hero image description"
                        }
                    }
                },
                TextModulesData = new List<TextModuleData>
                {
                    new TextModuleData
                    {
                        Header = "Text module header",
                        Body = "Text module body",
                        Id = "TEXT_MODULE_ID"
                    }
                },
                LinksModuleData = new LinksModuleData
                {
                    Uris = new List<Google.Apis.Walletobjects.v1.Data.Uri>
                    {
                        new Google.Apis.Walletobjects.v1.Data.Uri
                        {
                            UriValue = "http://maps.google.com/",
                            Description = "Link module URI description",
                            Id = "LINK_MODULE_URI_ID"
                        },
                        new Google.Apis.Walletobjects.v1.Data.Uri
                        {
                            UriValue = "tel:6505555555",
                            Description = "Link module tel description",
                            Id = "LINK_MODULE_TEL_ID"
                        }
                    }
                },
                ImageModulesData = new List<ImageModuleData>
                {
                    new ImageModuleData
                    {
                        MainImage = new Image
                        {
                            SourceUri = new ImageUri
                            {
                                Uri = "http://farm4.staticflickr.com/3738/12440799783_3dc3c20606_b.jpg"
                            },
                            ContentDescription = new LocalizedString
                            {
                                DefaultValue = new TranslatedString
                                {
                                    Language = "en-US",
                                    Value = "Image module description"
                                }
                            }
                        },
                        Id = "IMAGE_MODULE_ID"
                    }
                },
                Barcode = new Barcode
                {
                    Type = "QR_CODE",
                    Value = "QR code"
                },
                CardTitle = new LocalizedString
                {
                    DefaultValue = new TranslatedString
                    {
                        Language = "en-US",
                        Value = "Generic card title"
                    }
                },
                Header = new LocalizedString
                {
                    DefaultValue = new TranslatedString
                    {
                        Language = "en-US",
                        Value = "Generic header"
                    }
                },
                HexBackgroundColor = "#4285f4",
                Logo = new Image
                {
                    SourceUri = new ImageUri
                    {
                        Uri = "https://storage.googleapis.com/wallet-lab-tools-codelab-artifacts-public/pass_google_logo.jpg"
                    },
                    ContentDescription = new LocalizedString
                    {
                        DefaultValue = new TranslatedString
                        {
                            Language = "en-US",
                            Value = "Generic card logo"
                        }
                    }
                }
            };

            using (Stream responseStream = await _walletService.Genericobject
                .Insert(newObject)
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
