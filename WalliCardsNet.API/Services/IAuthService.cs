using WalliCardsNet.ClassLibrary.Login;
using WalliCardsNet.ClassLibrary.Register;
using static WalliCardsNet.API.Services.APIAuthService;

namespace WalliCardsNet.API.Services
{
    public interface IAuthService
    {

        Task<RegisterResponseDTO> RegisterEmployeeAsync(string name, string email, string password);

        Task<LoginResponseDTO> LoginAsync(string email, string password);
    }
}
