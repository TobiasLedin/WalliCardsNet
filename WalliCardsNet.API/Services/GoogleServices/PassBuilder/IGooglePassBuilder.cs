using Google.Apis.Walletobjects.v1.Data;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Services.GoogleServices.PassBuilder
{
    public interface IGooglePassBuilder
    {
        GenericClass BuildClassFromTemplate(BusinessProfile profile);
        GenericObject BuildObjectFromTemplate(BusinessProfile profile, Customer customer);

    }
}