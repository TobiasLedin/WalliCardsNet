using WalliCardsNet.ClassLibrary;
using static WalliCardsNet.API.Services.APIAuthService;

namespace WalliCardsNet.API.Services
{
    public interface IAuthService
    {

        Task<RegisterResultDTO> RegisterEmployeeAsync(string name, string email, string password);

        Task<LoginResultDTO> LoginAsync(string email, string password);
    }
}
