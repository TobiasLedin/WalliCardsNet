using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Json;
using WalliCardsNet.ClassLibrary;

namespace WalliCardsNet.Client.Services
{
    public class ClientAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public ClientAuthService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task LoginAsync(string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new LoginRequestDTO(email, password));

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResultDTO>();

                if (result.Token != null)
                {
                    await _localStorage.SetItemAsync("access-token", result.Token);
                }
            };
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("access-token");
        }

    }
}
