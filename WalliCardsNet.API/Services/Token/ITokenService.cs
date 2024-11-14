using System.Security.Claims;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Services.Token
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        Task<string> GenerateRefreshTokenAsync(Guid userId);
        Task DeleteRefreshTokenAsync(string refreshToken);
        Task<List<Claim>> GenerateClaimsAsync(ApplicationUser user, Business business);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
        Task<bool> RefreshTokenValidationAsync(string refreshToken);



    }
}