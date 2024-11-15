using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Walletobjects.v1.Data;
using Google.Apis.Walletobjects.v1;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Text.Json;
using WalliCardsNet.API.Models;
using WalliCardsNet.API.Services.GoogleServices.PassBuilder;
using Microsoft.AspNetCore.Connections.Features;

namespace WalliCardsNet.API.Services.GoogleServices.GoogleWallet
{
    /// <summary>
    /// Manages GoogleWallet API calls.
    /// </summary>
    /// Author: Tobias
    public class GoogleWalletService : IGoogleWallet
    {
        private readonly IGooglePassBuilder _googlePassBuilder;
        private readonly ILogger<GoogleWalletService> _logger;
        private WalletobjectsService _walletService;
        private string? _issuerId;
        private GenericClass? _genericClass;
        private GenericObject? _genericObject;

        public GoogleWalletService(IGooglePassBuilder googlePassBuilder, ILogger<GoogleWalletService> logger)
        {
            _walletService = GoogleWalletApiAuthentication();
            _googlePassBuilder = googlePassBuilder;
            _logger = logger;
        }


        /// <summary>
        /// Method corresponding to GenericClass GoogleWallet API INSERT call.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="classSuffix"></param>
        /// <returns></returns>
        public async Task<ActionResult<GenericClass>> CreateOrUpdateGenericClassAsync(BusinessProfile profile)
        {
            string classId = $"{_issuerId}.{profile.BusinessId}";

            if (await ClassExistsAsync(classId))
            {
                try
                {
                    _genericClass = _googlePassBuilder.BuildClassFromTemplate(profile);

                    using (Stream responseStream = await _walletService.Genericclass
                                                            .Update(_genericClass, classId)
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

                            _genericClass = JsonSerializer.Deserialize<GenericClass>(responseJson, SerializerOptions());

                            if (_genericClass?.Id == null)
                            {
                                return ActionResult<GenericClass>.FailureResult("Failed to deserialize response");
                            }

                            _logger.LogInformation($"Object {classId} successfully updated");

                            return ActionResult<GenericClass>.SuccessResult(_genericClass);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return ActionResult<GenericClass>.FailureResult($"Failed to update classId: {classId}", new List<string> { ex.Message });
                }
            }

            try
            {
                _genericClass = _googlePassBuilder.BuildClassFromTemplate(profile);

                using (Stream responseStream = await _walletService.Genericclass
                                                        .Insert(_genericClass)
                                                        .ExecuteAsStreamAsync())

                    var result = await ReadResponseStream(responseStream);

                

                //using (StreamReader responseReader = new StreamReader(responseStream))
                //{
                //    string responseJson = await responseReader.ReadToEndAsync();
                //    using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                //    {
                //        if (jsonResponse.RootElement.TryGetProperty("error", out JsonElement errorElement))
                //        {
                //            int errorCode = errorElement.GetProperty("code").GetInt32();
                //            string? errorMessage = errorElement.GetProperty("message").GetString();

                //            return ActionResult<GenericClass>.FailureResult(
                //                $"Google Wallet API returned following error(s) during Generic class INSERT call - Code: {errorCode}, Message: {errorMessage}");
                //        }

                //        _genericClass = JsonSerializer.Deserialize<GenericClass>(responseJson, SerializerOptions());

                //        if (_genericClass?.Id == null)
                //        {
                //            return ActionResult<GenericClass>.FailureResult("Failed to deserialize response");
                //        }

                //        _logger.LogInformation($"Class {classId} successfully created");

                //        return ActionResult<GenericClass>.SuccessResult(_genericClass);
                //    }
                //}
            }
            catch (Exception ex)
            {
                return ActionResult<GenericClass>.FailureResult("Failed to create GenericClass", new List<string> { ex.Message });
            }
        }

        // TEST
        private async Task<(int?, string?)> ReadResponseStream(Stream responseStream)
        {
            try
            {
                using (StreamReader responseReader = new StreamReader(responseStream))
                {
                    string responseJson = await responseReader.ReadToEndAsync();
                    using (JsonDocument jsonResponse = JsonDocument.Parse(responseJson))
                    {
                        if (jsonResponse.RootElement.TryGetProperty("error", out JsonElement errorElement))
                        {
                            int errorCode = errorElement.GetProperty("code").GetInt32();
                            string? errorMessage = errorElement.GetProperty("message").GetString();

                            return (errorCode, errorMessage);
                        }
                    }

                    var classJson = JsonSerializer.Deserialize<string>(responseJson, SerializerOptions());

                    return (null, classJson);
                }
            }
            catch (Exception ex)
            {
                return (1, ex.Message);
            }
        }

        /// <summary>
        /// Method corresponding to GenericClass GoogleWallet API UPDATE call.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="classSuffix"></param>
        /// <returns></returns>
        public async Task<ActionResult<GenericClass>> UpdateGenericClassAsync(BusinessProfile profile)
        {
            string classId = $"{_issuerId}.{profile.Id}";

            if (!await ClassExistsAsync(classId))
            {
                return ActionResult<GenericClass>.FailureResult($"No Generic class with id: {classId} exists");
            }

            try
            {
                _genericClass = _googlePassBuilder.BuildClassFromTemplate(profile);

                using (Stream responseStream = await _walletService.Genericclass
                                                        .Update(_genericClass, classId)
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

                        _genericClass = JsonSerializer.Deserialize<GenericClass>(responseJson, SerializerOptions());

                        if (_genericClass?.Id == null)
                        {
                            return ActionResult<GenericClass>.FailureResult("Failed to deserialize response");
                        }

                        _logger.LogInformation($"Object {classId} successfully updated");

                        return ActionResult<GenericClass>.SuccessResult(_genericClass);
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
        /// <param name="template"></param>
        /// <param name="customer"></param>
        /// <param name="objectSuffix"></param>
        /// <returns></returns>
        public async Task<ActionResult<GenericObject>> CreateGenericObjectAsync(BusinessProfile profile, Customer customer)
        {
            string objectId = $"{_issuerId}.{customer.Id}";

            if (await ObjectExistsAsync(objectId))
            {
                var result = await UpdateGenericObjectAsync(profile, customer);

                if (result.Success)
                {
                    _genericObject = result.Data;

                    if (_genericObject != null)
                    {
                        return ActionResult<GenericObject>.SuccessResult(_genericObject, $"Generic class with id: {objectId} already exists. Update of existing class performed");
                    }
                    else
                    {
                        return ActionResult<GenericObject>.FailureResult($"Failed to update existing class: {result.Message}");
                    }
                }
            }

            try
            {
                _genericObject = _googlePassBuilder.BuildObjectFromTemplate(profile, customer);

                using (Stream responseStream = await _walletService.Genericobject
                                                        .Insert(_genericObject)
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

                        _genericObject = JsonSerializer.Deserialize<GenericObject>(responseJson, SerializerOptions());

                        if (_genericObject?.Id == null)
                        {
                            return ActionResult<GenericObject>.FailureResult("Failed to deserialize Google Wallet API response");
                        }

                        return ActionResult<GenericObject>.SuccessResult(_genericObject);
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
        /// <param name="template"></param>
        /// <param name="customer"></param>
        /// <param name="objectSuffix"></param>
        /// <returns></returns>
        public async Task<ActionResult<GenericObject>> UpdateGenericObjectAsync(BusinessProfile profile, Customer customer)
        {
            string objectId = $"{_issuerId}.{customer.Id}";

            if (!await ObjectExistsAsync(objectId))
            {
                return ActionResult<GenericObject>.FailureResult($"No Generic object with id: {objectId} exists");
            }

            try
            {
                _genericObject = _googlePassBuilder.BuildObjectFromTemplate(profile, customer);

                using (Stream responseStream = await _walletService.Genericobject
                                                        .Update(_genericObject, objectId)
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

                        _genericObject = JsonSerializer.Deserialize<GenericObject>(responseJson, SerializerOptions());

                        if (_genericObject?.Id == null)
                        {
                            return ActionResult<GenericObject>.FailureResult("Failed to deserialize Google Wallet API response");
                        }

                        _logger.LogInformation($"Object {objectId} successfully updated");

                        return ActionResult<GenericObject>.SuccessResult(_genericObject);
                    }
                }
            }
            catch (Exception ex)
            {
                return ActionResult<GenericObject>.FailureResult($"Failed to update objectId: {objectId} ", new List<string> { ex.Message });
            }
        }


        /// <summary>
        /// Creates a signed JWT with reference to a GenericObject and validates it against GoogleWallet API.
        /// </summary>
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
                var jwtResource = new Google.Apis.Walletobjects.v1.Data.JwtResource
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

                            return ActionResult<string>.SuccessResult(saveUri!);
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


        /// <summary>
        /// Check if GenericClass with classId exists.
        /// </summary>
        /// <param name="classId"></param>
        /// <returns>true if classId exists, false if not.</returns>
        private async Task<bool> ClassExistsAsync(string classId)
        {
            using (Stream responseStream = await _walletService.Genericclass
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
        /// Authenticate with GoogleWallet API and instansiate WalletService.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        private WalletobjectsService GoogleWalletApiAuthentication()
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

            return new WalletobjectsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials
            });
        }


        /// <summary>
        /// Set up JsonSerializer options.
        /// </summary>
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
    }
}
