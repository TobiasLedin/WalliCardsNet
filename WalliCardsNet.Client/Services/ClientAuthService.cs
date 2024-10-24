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

        public async Task LoginAsync(string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("login", new LoginRequest(email, password));

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>() ?? throw new NullReferenceException("Unable to read Http response");

                if (result.AccessToken != null)
                {
                    await _localStorage.SetItemAsync("accessToken", result.AccessToken);
                    await _authState.GetAuthenticationStateAsync();
                }
            }
        }

        public async Task LinkGoogleAccountAsync(string code)
        {
            var response = await _httpClient.PostAsJsonAsync("link/google", code);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Something went wrong");
            }
        }

        public async Task LoginGoogleAsync(string code)
        {
            var response = await _httpClient.PostAsJsonAsync("google", code);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.AccessToken != null)
                {
                    await _localStorage.SetItemAsync("accessToken", result.AccessToken);
                    await _authState.GetAuthenticationStateAsync();
                }
            }
            else
            {
                throw new Exception("Login failed");
            }
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

            await _httpClient.SendAsync(request);
            await _authState.LogoutAsync();
        }
    }
}
