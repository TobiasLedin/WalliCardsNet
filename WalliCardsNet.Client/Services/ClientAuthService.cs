using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using WalliCardsNet.ClassLibrary.Login;

namespace WalliCardsNet.Client.Services
{
    public class ClientAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthStateProvider _authState;

        public ClientAuthService(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage, AuthStateProvider authState)
        {
            _httpClient = httpClientFactory.CreateClient("WalliCardsApi");
            _localStorage = localStorage;
            _authState = authState;
        }

        public async Task LoginAsync(string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new LoginRequestDTO(email, password));

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDTO>() ?? throw new NullReferenceException("Unable to read Http response");

                if (result.Token != null)
                {
                    await _localStorage.SetItemAsync("access-token", result.Token);
                    await _authState.GetAuthenticationStateAsync();
                }
            };
        }

        public async Task LinkGoogleAccountAsync(string code)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/link/google", code);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Something went wrong");
            }
        }

        public async Task LoginGoogleAsync(string code)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/google", code);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
                if (result?.Token != null)
                {
                    await _localStorage.SetItemAsync("access-token", result.Token);
                    await _authState.GetAuthenticationStateAsync();
                }
            }
            else
            {
                throw new Exception("Login failed");
            }
        }

        public async Task LogoutAsync()
        {
            await _authState.LogoutAsync();

        }

    }
}
