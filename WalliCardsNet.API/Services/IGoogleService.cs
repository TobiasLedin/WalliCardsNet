using Google.Apis.Walletobjects.v1.Data;
using System.Text.Json;
using WalliCardsNet.API.Models;

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
        Task<ActionResult<string>> CreateSignedJWTAsync(GooglePass pass);
        JsonSerializerOptions SerializerOptions();
    }
}
