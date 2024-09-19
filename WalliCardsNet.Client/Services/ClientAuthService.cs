using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Json;
using WalliCardsNet.ClassLibrary;

namespace WalliCardsNet.Client.Services
{
    public class ClientAuthService
    {
        private readonly HttpClient _httpClient;

        public ClientAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task Login(string email, string password)
        {

            var loginResponse = await _httpClient.PostAsJsonAsync("api/auth/login", new LoginRequestDTO(email, password));

            if (loginResponse.IsSuccessStatusCode)
            {
                var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthorizationResult>();
                
            };
            
        }

    }
}
