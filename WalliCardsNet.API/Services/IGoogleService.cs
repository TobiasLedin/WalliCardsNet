using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Services
{
    public interface IGoogleService
    {
        Task<Dictionary<string, object>> ExchangeCodeForTokensAsync(string code, string redirectUri);
        (string googleUserId, string googleEmail) DecodeIdToken(string idToken);
        Task<bool> LinkGoogleAccountAsync(ApplicationUser user, string googleUserId);

        Task<string> CreateGenericClassAsync(string classSuffix);
        Task<string> CreateGenericObjectAsync(string classSuffix, string objectSuffix);
    }
}
