using Google.Apis.Walletobjects.v1.Data;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Services.GoogleServices.GoogleWallet
{
    public interface IGoogleWallet
    {
        Task<ActionResult<GenericClass>> CreateOrUpdateGenericClassAsync(BusinessProfile profile);
        Task<ActionResult<GenericObject>> CreateGenericObjectAsync(BusinessProfile profile, Customer customer);
        Task<ActionResult<GenericObject>> UpdateGenericObjectAsync(BusinessProfile profile, Customer customer);
        Task<ActionResult<string>> CreateSignedJWTAsync(GooglePass pass);
        Task<List<string>> BatchUpdateGenericObjectsAsync(BusinessProfile profile, List<Customer> customers);

    }
}