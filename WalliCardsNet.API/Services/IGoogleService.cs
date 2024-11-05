using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.BusinessProfile;

namespace WalliCardsNet.API.Services
{
    public interface IGoogleService
    {
        Task<Dictionary<string, object>> ExchangeCodeForTokensAsync(string code, string redirectUri);
        (string googleUserId, string googleEmail) DecodeIdToken(string idToken);
        Task<bool> LinkGoogleAccountAsync(ApplicationUser user, string googleUserId);

        Task<string> CreateGenericClassAsync(GooglePassTemplate template, string classSuffix);
        Task<string> CreateGenericObjectAsync(GooglePassTemplate template, Customer customer, string classSuffix, string objectSuffix);
    }
}
