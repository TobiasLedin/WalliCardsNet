using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Sprache;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using WalliCardsNet.ClassLibrary.Login;
using WalliCardsNet.Client.Models;

namespace WalliCardsNet.Client.Services
{
    public class ClientAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthStateProvider _authState;
        

        public ClientAuthService(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage, AuthStateProvider authState)
        {
            _httpClient = httpClientFactory.CreateClient("AuthClient");
            _localStorage = localStorage;
            _authState = authState;
        }

        public async Task<LoginSuccessCheck> LoginAsync(string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("login", new LoginRequest(email, password));
            var loginSuccessCheck = new LoginSuccessCheck();
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>() ?? throw new NullReferenceException("Unable to read Http response");

                if (result.AccessToken != null)
                {
                    await _localStorage.SetItemAsync("accessToken", result.AccessToken);
                    await _authState.GetAuthenticationStateAsync();
                    loginSuccessCheck.Success = true;
                    return loginSuccessCheck;
                }
            }
            loginSuccessCheck.Success = false;
            return loginSuccessCheck;
        }

        public async Task<bool> LinkGoogleAccountAsync(string code)
        {
            var response = await _httpClient.PostAsJsonAsync("link/google", code);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            return true;
        }

        public async Task<LoginSuccessCheck> LoginGoogleAsync(string code)
        {
            var loginSuccessCheck = new LoginSuccessCheck();
            var response = await _httpClient.PostAsJsonAsync("google", code);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.AccessToken != null)
                {
                    await _localStorage.SetItemAsync("accessToken", result.AccessToken);
                    await _authState.GetAuthenticationStateAsync();
                    loginSuccessCheck.Success = true;
                    loginSuccessCheck.Details = "";
                    return loginSuccessCheck;
                }
            }
            loginSuccessCheck.Success = false;
            loginSuccessCheck.Details = "Something went wrong";
            return loginSuccessCheck;
        }

        public async Task<RefreshResult> RefreshAccessTokenAsync()
        {
            try
            {
                var accessToken = await _localStorage.GetItemAsync<string>("accessToken");

                if (!string.IsNullOrEmpty(accessToken))
                {
                    var response = await _httpClient.PostAsJsonAsync("refresh-token", accessToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                        if (result?.AccessToken != null)
                        {
                            await _localStorage.SetItemAsync("accessToken", result.AccessToken);
                            await _authState.GetAuthenticationStateAsync();

                            return new RefreshResult { Success = true, Token = result.AccessToken };
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Unable to refresh token");
            }

            return new RefreshResult { Success = false };
        }

        public async Task LogoutAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "logout");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                await _authState.LogoutAsync();
            }
        }
    }
}
