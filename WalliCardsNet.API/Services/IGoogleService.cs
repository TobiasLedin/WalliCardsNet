using Google.Apis.Walletobjects.v1.Data;
using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.BusinessProfile;

namespace WalliCardsNet.API.Services
{
    public interface IGoogleService
    {
        Task<Dictionary<string, object>> ExchangeCodeForTokensAsync(string code, string redirectUri);
        (string googleUserId, string googleEmail) DecodeIdToken(string idToken);
        Task<bool> LinkGoogleAccountAsync(ApplicationUser user, string googleUserId);

        Task<ActionResult<GenericClass>> CreateGenericClassAsync(GooglePassTemplate template, string classSuffix);
        Task<ActionResult<GenericObject>> CreateGenericObjectAsync(GooglePassTemplate template, Customer customer, string objectSuffix);
        Task<ActionResult<GenericObject>> UpdateGenericObjectAsync(GooglePassTemplate template, Customer customer, string objectSuffix);
        Task<ActionResult<GenericClass>> UpdateGenericClassAsync(GooglePassTemplate template, string classSuffix);
    }
}
