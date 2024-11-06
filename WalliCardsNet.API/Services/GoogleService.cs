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
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

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

        #region GoogleWallet Pass

        /// <summary>
        /// Authenticate with GoogleWallet API and instansiate WalletService.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public void GoogleWalletApiAuthentication()
        {
            _issuerId = Environment.GetEnvironmentVariable("GOOGLE-WALLET-ISSUER-ID") ?? throw new NullReferenceException("Not able to load IssuerId");
            var keyFilePath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS") ?? throw new NullReferenceException("Not able to load Google application credentials");

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

        /// <summary>
        /// Method corresponding to GenericClass GoogleWallet API INSERT call.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="classSuffix"></param>
        /// <returns></returns>
        public async Task<ActionResult<GenericClass>> CreateGenericClassAsync(GooglePassTemplate template, string classSuffix)
        {
            if (_walletService == null)
            {
                GoogleWalletApiAuthentication();
            }

            string classId = $"{_issuerId}.{classSuffix}";
            GenericClass? genericClass;

            if (await ClassExists(classId))
            {
                return ActionResult<GenericClass>.FailureResult($"A GenericClass with id: {classId} already exists");
            }

            try
            {
                using (Stream responseStream = await _walletService!.Genericclass
                    .Insert(new GooglePassBuilder().BuildClassFromTemplate(template))
                    .ExecuteAsStreamAsync())

                using (StreamReader responseReader = new StreamReader(responseStream))
                {
                    string responseJson = await responseReader.ReadToEndAsync();
                    using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                    {
                        genericClass = JsonSerializer.Deserialize<GenericClass>(responseJson, SerializerOptions());

                        if (genericClass == null)
                        {
                            return ActionResult<GenericClass>.FailureResult("Failed to deserialize response");
                        }

                        _logger.LogInformation($"Class {classId} successfully created");

                        return ActionResult<GenericClass>.SuccessResult(genericClass);
                    }
                }
            }
            catch (Exception ex)
            {
                return ActionResult<GenericClass>.FailureResult("An error occured", new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// Method corresponding to GenericObject GoogleWallet API INSERT call.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="customer"></param>
        /// <param name="objectSuffix"></param>
        /// <returns></returns>
        public async Task<ActionResult<GenericObject>> CreateGenericObjectAsync(GooglePassTemplate template, Customer customer, string objectSuffix) // ClassSuffix: BusinessId?, ObjectSuffix: CustomerId?
        {
            if (_walletService == null)
            {
                GoogleWalletApiAuthentication();
            }

            string objectId = $"{_issuerId}.{objectSuffix}";
            GenericObject? genericObject;

            if(await ObjectExists(objectId))
            {
                return ActionResult<GenericObject>.FailureResult($"A GenericObject with id: {objectId} already exists");
            }

            try
            {
                using (Stream responseStream = await _walletService!.Genericobject
                    .Insert(new GooglePassBuilder().BuildObjectFromTemplate(template, customer))
                    .ExecuteAsStreamAsync())

                using (StreamReader responseReader = new StreamReader(responseStream))
                {
                    string responseJson = await responseReader.ReadToEndAsync();
                    using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                    {
                        genericObject = JsonSerializer.Deserialize<GenericObject>(responseJson, SerializerOptions());

                        if (genericObject == null)
                        {
                            return ActionResult<GenericObject>.FailureResult("Failed to deserialize response");
                        }

                        _logger.LogInformation($"Object {objectId} successfully created");

                        return ActionResult<GenericObject>.SuccessResult(genericObject);
                    }
                }
            }
            catch (Exception ex)
            {
                return ActionResult<GenericObject>.FailureResult("An error occured", new List<string> { ex.Message } );
            } 
        }

        /// <summary>
        /// Method corresponding to GenericObject GoogleWallet API UPDATE call.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="customer"></param>
        /// <param name="objectSuffix"></param>
        /// <returns></returns>
        public async Task<ActionResult<GenericObject>> UpdateGenericObjectAsync(GooglePassTemplate template, Customer customer, string objectSuffix)
        {
            if (_walletService == null)
            {
                GoogleWalletApiAuthentication();
            }

            string objectId = $"{_issuerId}.{objectSuffix}";
            GenericObject? genericObject;

            if (!await ObjectExists(objectId))
            {
                return ActionResult<GenericObject>.FailureResult($"No GenericObject with id: {objectId} exists");
            }

            try
            {
                using (Stream responseStream = await _walletService!.Genericobject
                    .Update(new GooglePassBuilder().BuildObjectFromTemplate(template, customer), objectId)
                    .ExecuteAsStreamAsync())

                using (StreamReader responseReader = new StreamReader(responseStream))
                {
                    string responseJson = await responseReader.ReadToEndAsync();
                    using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                    {
                        genericObject = JsonSerializer.Deserialize<GenericObject>(responseJson, SerializerOptions());

                        if (genericObject == null)
                        {
                            return ActionResult<GenericObject>.FailureResult("Failed to deserialize response");
                        }

                        _logger.LogInformation($"Object {objectId} successfully updated");

                        return ActionResult<GenericObject>.SuccessResult(genericObject);
                    }
                }
            }
            catch (Exception ex)
            {
                return ActionResult<GenericObject>.FailureResult("An error occured", new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// Method corresponding to GenericClass GoogleWallet API UPDATE call.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="classSuffix"></param>
        /// <returns></returns>
        public async Task<ActionResult<GenericClass>> UpdateGenericClassAsync(GooglePassTemplate template, string classSuffix)
        {
            if (_walletService == null)
            {
                GoogleWalletApiAuthentication();
            }

            string objectId = $"{_issuerId}.{classSuffix}";
            GenericClass? genericClass;

            if (!await ObjectExists(objectId))
            {
                return ActionResult<GenericClass>.FailureResult($"No GenericObject with id: {objectId} exists");
            }

            try
            {
                using (Stream responseStream = await _walletService!.Genericclass
                    .Update(new GooglePassBuilder().BuildClassFromTemplate(template), objectId)
                    .ExecuteAsStreamAsync())

                using (StreamReader responseReader = new StreamReader(responseStream))
                {
                    string responseJson = await responseReader.ReadToEndAsync();
                    using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                    {
                        genericClass = JsonSerializer.Deserialize<GenericClass>(responseJson, SerializerOptions());

                        if (genericClass == null)
                        {
                            return ActionResult<GenericClass>.FailureResult("Failed to deserialize response");
                        }

                        _logger.LogInformation($"Object {objectId} successfully updated");

                        return ActionResult<GenericClass>.SuccessResult(genericClass);
                    }
                }
            }
            catch (Exception ex)
            {
                return ActionResult<GenericClass>.FailureResult("An error occured", new List<string> { ex.Message });
            }
        }

        public ActionResult<string> CreateSignedJWT(GenericClass genericClass, GenericObject genericObject)
        {
            var serviceAccountEmail = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_SERVICE_ACCOUNT_EMAIL") ?? throw new NullReferenceException("Not able to load Google Cloud service account email");
            var keyFilePath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS") ?? throw new NullReferenceException("Not able to load Google application credentials");
            var devTunnel = Environment.GetEnvironmentVariable("DEV_TUNNEL_ADRESS") ?? throw new NullReferenceException("Not able to load Dev-tunnel adress");

            try
            {
                using var classJson = JsonDocument.Parse(JsonSerializer.Serialize(genericClass, SerializerOptions()));
                using var objectJson = JsonDocument.Parse(JsonSerializer.Serialize(genericObject, SerializerOptions()));

                using var jwtPayload = JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    iss = serviceAccountEmail,
                    aud = "google",
                    typ = "savetowallet",
                    iat = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    origins = new List<string>
                {
                    devTunnel
                },
                    payload = JsonDocument.Parse(JsonSerializer.Serialize(new
                    {
                        loyaltyClasses = new List<JsonDocument>
                    {
                        classJson
                    },
                        loyaltyObjects = new List<JsonDocument>
                    {
                        objectJson
                    }
                    }))
                }));

                JwtPayload claims = JwtPayload.Deserialize(jwtPayload.ToString());

                string key = File.ReadAllText(keyFilePath);
                var keyJson = JsonDocument.Parse(key);
                string privateKey = keyJson.RootElement.GetProperty("private_key").GetString() ?? throw new InvalidOperationException("Private key not found in key file");

                RSA rsa = RSA.Create();
                rsa.ImportFromPem(privateKey.ToCharArray());
                RsaSecurityKey rsaKey = new RsaSecurityKey(rsa);

                SigningCredentials signingCredentials = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);

                JwtSecurityToken jwt = new JwtSecurityToken(new JwtHeader(signingCredentials), claims);

                string token = new JwtSecurityTokenHandler().WriteToken(jwt);

                _logger.LogInformation("Add to Google Wallet link generated");
                _logger.LogInformation($"https://pay.google.com/gp/v/save/{token}");

                return ActionResult<string>.SuccessResult($"https://pay.google.com/gp/v/save/{token}");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                return ActionResult<string>.FailureResult("Failed to generate JWT", new List<string> { ex.Message });
            }
        }

        // Support methods
        private async Task<bool> ClassExists(string classId)
        {
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
                        return true;
                    }
                    else if (errorElement.GetProperty("code").GetInt32() != 404)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private async Task<bool> ObjectExists(string objectId)
        {
            using (Stream responseStream = await _walletService!.Genericobject
                .Get(objectId)
                .ExecuteAsStreamAsync())

            using (StreamReader responseReader = new StreamReader(responseStream))
            {
                string responseJson = await responseReader.ReadToEndAsync();
                using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                {
                    JsonElement rootElement = jsonResponse.RootElement;
                    if (!rootElement.TryGetProperty("error", out JsonElement errorElement))
                    {
                        return true;
                    }
                    else if (errorElement.GetProperty("code").GetInt32() != 404)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private JsonSerializerOptions SerializerOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                AllowTrailingCommas = true
            };
        }

        #endregion
    }
}
