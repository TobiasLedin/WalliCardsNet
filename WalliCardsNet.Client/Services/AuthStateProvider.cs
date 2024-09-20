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
            // No JWT - return empty ClaimsPrincipal

            var accessToken = await _localStorage.GetItemAsStringAsync("access-token");

            if (string.IsNullOrEmpty(accessToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())); // Empty ClaimsPrincipal
            }

            // JWT expired - return empty ClaimsPrincipal

            var tokenHandler = new JwtSecurityTokenHandler();

            var jwtContent = tokenHandler.ReadJwtToken(accessToken);

            if (jwtContent.ValidTo < DateTime.UtcNow)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())); // Empty ClaimsPrincipal
            }

            // JWT Valid - add JWT claims to ClaimsPrinciple

            var claimsIdentity = new ClaimsIdentity(jwtContent.Claims, "jwt");

            var user = new ClaimsPrincipal(claimsIdentity);

            // NotifyAuthStateChanged

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));

            // Return authenticated user

            return new AuthenticationState(user);
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("access-token");

            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
        }
    }
}
