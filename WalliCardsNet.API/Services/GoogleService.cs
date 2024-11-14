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
        private readonly ILogger<GoogleService> _logger;
        private WalletobjectsService? _walletService;
        private string? _issuerId;

        public GoogleService(UserManager<ApplicationUser> userManager, ILogger<GoogleService> logger)
        {
            _userManager = userManager;
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
        /// <remarks>Author: Tobias</remarks>
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
        /// <remarks>Author: Tobias</remarks>
        /// <param name="template"></param>
        /// <param name="classSuffix"></param>
        /// <returns></returns>
        public async Task<ActionResult<GenericClass>> CreateGenericClassAsync(BusinessProfile profile)
        {
            if (_walletService == null)
            {
                GoogleWalletApiAuthentication();
            }

            string classId = $"{_issuerId}.{profile.Id}";
            GenericClass? genericClass;

            if (await ClassExistsAsync(classId))
            {
                var result = await UpdateGenericClassAsync(profile);

                if (result.Success)
                {
                    genericClass = result.Data;

                    if (genericClass != null)
                    {
                        return ActionResult<GenericClass>.SuccessResult(genericClass, $"Generic class with id: {classId} already exists. Update of existing class performed");
                    }
                    else
                    {
                        return ActionResult<GenericClass>.FailureResult($"Failed to update existing class: {result.Message}");
                    }
                }
            }

            try
            {
                var builderProduct = new GooglePassBuilder().BuildClassFromTemplate(profile);

                using (Stream responseStream = await _walletService!.Genericclass
                    .Insert(builderProduct)
                    .ExecuteAsStreamAsync())

                using (StreamReader responseReader = new StreamReader(responseStream))
                {
                    string responseJson = await responseReader.ReadToEndAsync();
                    using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                    {
                        if (jsonResponse.RootElement.TryGetProperty("error", out JsonElement errorElement))
                        {
                            int errorCode = errorElement.GetProperty("code").GetInt32();
                            string? errorMessage = errorElement.GetProperty("message").GetString();

                            return ActionResult<GenericClass>.FailureResult(
                                $"Google Wallet API returned following error(s) during Generic class INSERT call - Code: {errorCode}, Message: {errorMessage}");
                        }

                        genericClass = JsonSerializer.Deserialize<GenericClass>(responseJson, SerializerOptions());

                        if (genericClass?.Id == null)
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
                return ActionResult<GenericClass>.FailureResult("Failed to create GenericClass", new List<string> { ex.Message });
            }
        }


        /// <summary>
        /// Method corresponding to GenericClass GoogleWallet API UPDATE call.
        /// </summary>
        /// <remarks>Author: Tobias</remarks>
        /// <param name="template"></param>
        /// <param name="classSuffix"></param>
        /// <returns></returns>
        public async Task<ActionResult<GenericClass>> UpdateGenericClassAsync(BusinessProfile profile)
        {
            if (_walletService == null)
            {
                GoogleWalletApiAuthentication();
            }

            string classId = $"{_issuerId}.{profile.Id}";
            GenericClass? genericClass;

            if (!await ClassExistsAsync(classId))
            {
                return ActionResult<GenericClass>.FailureResult($"No Generic class with id: {classId} exists");
            }

            try
            {
                var builderProduct = new GooglePassBuilder().BuildClassFromTemplate(profile);

                using (Stream responseStream = await _walletService!.Genericclass
                    .Update(builderProduct, classId)
                    .ExecuteAsStreamAsync())

                using (StreamReader responseReader = new StreamReader(responseStream))
                {
                    string responseJson = await responseReader.ReadToEndAsync();
                    using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                    {
                        if (jsonResponse.RootElement.TryGetProperty("error", out JsonElement errorElement))
                        {
                            int errorCode = errorElement.GetProperty("code").GetInt32();
                            string? errorMessage = errorElement.GetProperty("message").GetString();

                            return ActionResult<GenericClass>.FailureResult(
                                $"Google Wallet API returned following error(s) during Generic class UPDATE call - Code: {errorCode}, Message: {errorMessage}");
                        }

                        genericClass = JsonSerializer.Deserialize<GenericClass>(responseJson, SerializerOptions());

                        if (genericClass?.Id == null)
                        {
                            return ActionResult<GenericClass>.FailureResult("Failed to deserialize response");
                        }

                        _logger.LogInformation($"Object {classId} successfully updated");

                        return ActionResult<GenericClass>.SuccessResult(genericClass);
                    }
                }
            }
            catch (Exception ex)
            {
                return ActionResult<GenericClass>.FailureResult($"Failed to update classId: {classId}", new List<string> { ex.Message });
            }
        }


        /// <summary>
        /// Method corresponding to GenericObject GoogleWallet API INSERT call.
        /// </summary>
        /// <remarks>Author: Tobias</remarks>
        /// <param name="template"></param>
        /// <param name="customer"></param>
        /// <param name="objectSuffix"></param>
        /// <returns></returns>
        public async Task<ActionResult<GenericObject>> CreateGenericObjectAsync(BusinessProfile profile, Customer customer)
        {
            if (_walletService == null)
            {
                GoogleWalletApiAuthentication();
            }

            string objectId = $"{_issuerId}.{customer.Id}";
            GenericObject? genericObject;

            if (await ObjectExistsAsync(objectId))
            {
                var result = await UpdateGenericObjectAsync(profile, customer);

                if (result.Success)
                {
                    genericObject = result.Data;

                    if (genericObject != null)
                    {
                        return ActionResult<GenericObject>.SuccessResult(genericObject, $"Generic class with id: {objectId} already exists. Update of existing class performed");
                    }
                    else
                    {
                        return ActionResult<GenericObject>.FailureResult($"Failed to update existing class: {result.Message}");
                    }
                }
            }

            try
            {
                var builderProduct = new GooglePassBuilder().BuildObjectFromTemplate(profile, customer);

                using (Stream responseStream = await _walletService!.Genericobject
                    .Insert(builderProduct)
                    .ExecuteAsStreamAsync())

                using (StreamReader responseReader = new StreamReader(responseStream))
                {
                    string responseJson = await responseReader.ReadToEndAsync();
                    using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                    {
                        if (jsonResponse.RootElement.TryGetProperty("error", out JsonElement errorElement))
                        {
                            int errorCode = errorElement.GetProperty("code").GetInt32();
                            string? errorMessage = errorElement.GetProperty("message").GetString();

                            return ActionResult<GenericObject>.FailureResult(
                                $"Google Wallet API returned following error(s) during Generic object INSERT call - Code: {errorCode}, Message: {errorMessage}");
                        }

                        genericObject = JsonSerializer.Deserialize<GenericObject>(responseJson, SerializerOptions());

                        if (genericObject?.Id == null)
                        {
                            return ActionResult<GenericObject>.FailureResult("Failed to deserialize Google Wallet API response");
                        }

                        return ActionResult<GenericObject>.SuccessResult(genericObject);
                    }
                }
            }
            catch (Exception ex)
            {
                return ActionResult<GenericObject>.FailureResult("Failed to create GenericObject", new List<string> { ex.Message });
            }
        }


        /// <summary>
        /// Method corresponding to GenericObject GoogleWallet API UPDATE call.
        /// </summary>
        /// <remarks>Author: Tobias</remarks>
        /// <param name="template"></param>
        /// <param name="customer"></param>
        /// <param name="objectSuffix"></param>
        /// <returns></returns>
        public async Task<ActionResult<GenericObject>> UpdateGenericObjectAsync(BusinessProfile profile, Customer customer)
        {
            if (_walletService == null)
            {
                GoogleWalletApiAuthentication();
            }

            string objectId = $"{_issuerId}.{customer.Id}";
            GenericObject genericObject;

            if (!await ObjectExistsAsync(objectId))
            {
                return ActionResult<GenericObject>.FailureResult($"No Generic object with id: {objectId} exists");
            }

            try
            {
                var builderProduct = new GooglePassBuilder().BuildObjectFromTemplate(profile, customer);

                using (Stream responseStream = await _walletService!.Genericobject
                    .Update(builderProduct, objectId)
                    .ExecuteAsStreamAsync())

                using (StreamReader responseReader = new StreamReader(responseStream))
                {
                    string responseJson = await responseReader.ReadToEndAsync();
                    using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                    {
                        if (jsonResponse.RootElement.TryGetProperty("error", out JsonElement errorElement))
                        {
                            int errorCode = errorElement.GetProperty("code").GetInt32();
                            string? errorMessage = errorElement.GetProperty("message").GetString();

                            return ActionResult<GenericObject>.FailureResult(
                                $"Google Wallet API returned following error(s) during Generic object UPDATE call - Code: {errorCode}, Message: {errorMessage}");
                        }

                        genericObject = JsonSerializer.Deserialize<GenericObject>(responseJson, SerializerOptions());

                        if (genericObject?.Id == null)
                        {
                            return ActionResult<GenericObject>.FailureResult("Failed to deserialize Google Wallet API response");
                        }

                        _logger.LogInformation($"Object {objectId} successfully updated");

                        return ActionResult<GenericObject>.SuccessResult(genericObject);
                    }
                }
            }
            catch (Exception ex)
            {
                return ActionResult<GenericObject>.FailureResult($"Failed to update objectId: {objectId} ", new List<string> { ex.Message });
            }
        }


        /// <summary>
        /// Creates a signed JWT with reference to a GenericClass and GenericObject.
        /// </summary>
        /// <remarks>Author: Tobias</remarks>
        /// <param name="genericClass"></param>
        /// <param name="genericObject"></param>
        /// <returns>String link to embedd in "Add to Google Wallet" button.</returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<ActionResult<string>> CreateSignedJWTAsync(GooglePass pass)
        {
            var serviceAccountEmail = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_SERVICE_ACCOUNT_EMAIL") ?? throw new NullReferenceException("Not able to load Google Cloud service account email");
            var keyFilePath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS") ?? throw new NullReferenceException("Not able to load Google application credentials");
            var devTunnel = Environment.GetEnvironmentVariable("DEV_TUNNEL_ADDRESS") ?? throw new NullReferenceException("Not able to load Dev-tunnel adress");

            try
            {
                //using var classJson = JsonDocument.Parse(genericClassJson);
                //using var objectJson = JsonDocument.Parse(genericObjectJson);

                var payload = JsonDocument.Parse(JsonSerializer.Serialize(new
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
                        genericObjects = new[]
                        {
                            new
                            {
                                id = pass.ObjectId,
                            }
                        }
                    }))
                }));

                string payloadJson = JsonSerializer.Serialize(payload);
                JwtPayload claims = JwtPayload.Deserialize(payloadJson);

                string key = File.ReadAllText(keyFilePath);
                var keyJson = JsonDocument.Parse(key);
                string privateKey = keyJson.RootElement.GetProperty("private_key").GetString() ?? throw new InvalidOperationException("Private key not found in key file");

                RSA rsa = RSA.Create();
                rsa.ImportFromPem(privateKey.ToCharArray());
                RsaSecurityKey rsaKey = new RsaSecurityKey(rsa);

                SigningCredentials signingCredentials = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);

                JwtSecurityToken jwt = new JwtSecurityToken(new JwtHeader(signingCredentials), claims);

                string signedToken = new JwtSecurityTokenHandler().WriteToken(jwt);


                // Validate JWT against Google Wallet API

                var jwtResource = new Google.Apis.Walletobjects.v1.Data.JwtResource()
                {
                    Jwt = signedToken
                };

                using (Stream responseStream = await _walletService!.Jwt
                    .Insert(jwtResource)
                    .ExecuteAsStreamAsync())

                using (StreamReader responseReader = new StreamReader(responseStream))
                {
                    string responseJson = await responseReader.ReadToEndAsync();
                    using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                    {
                        if (jsonResponse.RootElement.TryGetProperty("error", out JsonElement errorElement))
                        {
                            int errorCode = errorElement.GetProperty("code").GetInt32();
                            string? errorMessage = errorElement.GetProperty("message").GetString();

                            return ActionResult<string>.FailureResult(
                                $"Google Wallet API returned following error(s) during JWT INSERT - Code: {errorCode}, Message: {errorMessage}");
                        }
                        else if (jsonResponse.RootElement.TryGetProperty("saveUri", out JsonElement saveElement))
                        {
                            string? saveUri = saveElement.GetString();

                            _logger.LogInformation("Add to Google Wallet link generated");
                            _logger.LogInformation(saveUri);

                            return ActionResult<string>.SuccessResult(saveUri);
                        }
                        else
                        {
                            return ActionResult<string>.FailureResult("Something failed when generating JWT!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return ActionResult<string>.FailureResult("Failed to generate JWT", new List<string> { ex.Message });
            }
        }

        #endregion

        #region Support methods

        /// <summary>
        /// Check if GenericClass with classId exists.
        /// </summary>
        /// <remarks>Author: Tobias</remarks>
        /// <param name="classId"></param>
        /// <returns>true if classId exists, false if not.</returns>
        private async Task<bool> ClassExistsAsync(string classId)
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


        /// <summary>
        /// Check if GenericObject with objectId exists.
        /// </summary>
        /// <remarks>Author: Tobias</remarks>
        /// <param name="objectId"></param>
        /// <returns>true if objectId exists, false if not.</returns>
        private async Task<bool> ObjectExistsAsync(string objectId)
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


        /// <summary>
        /// Set up JsonSerializer options.
        /// </summary>
        /// <remarks>Author: Tobias</remarks>
        /// <returns>JsonSerializerOptions</returns>
        public JsonSerializerOptions SerializerOptions()
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
