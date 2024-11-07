using Google.Apis.Walletobjects.v1.Data;
using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.BusinessProfile;
using WalliCardsNet.ClassLibrary.BusinessProfile.Models;

namespace WalliCardsNet.API.Services
{
    public interface IGoogleService
    {
        Task<Dictionary<string, object>> ExchangeCodeForTokensAsync(string code, string redirectUri);
        (string googleUserId, string googleEmail) DecodeIdToken(string idToken);
        Task<bool> LinkGoogleAccountAsync(ApplicationUser user, string googleUserId);

        Task<ActionResult<GenericClass>> CreateGenericClassAsync(BusinessProfile profile);
        Task<ActionResult<GenericObject>> CreateGenericObjectAsync(BusinessProfile profile, Customer customer);
        Task<ActionResult<GenericObject>> UpdateGenericObjectAsync(BusinessProfile profile, Customer customer);
        Task<ActionResult<GenericClass>> UpdateGenericClassAsync(BusinessProfile profile);
    }
}
