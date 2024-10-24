using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WalliCardsNet.Client.Services
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;

        public AuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var anonymousUser = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            var accessToken = await _localStorage.GetItemAsync<string>("accessToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                return anonymousUser;
            }
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtContent = new JwtSecurityToken();

            try
            {
                jwtContent = tokenHandler.ReadJwtToken(accessToken);
            }
            catch (Exception)
            {
                Console.WriteLine("Not able to read JWT");
                await _localStorage.RemoveItemAsync("accessToken");

                return anonymousUser;
            }

            if (jwtContent.ValidTo < DateTime.UtcNow)  // Add fresh-token integration?
            {
                return anonymousUser;
            }

            var claimsIdentity = new ClaimsIdentity(jwtContent.Claims, "jwt");
            var user = new ClaimsPrincipal(claimsIdentity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));

            return new AuthenticationState(user);
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("accessToken");

            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
        }
    }
}
