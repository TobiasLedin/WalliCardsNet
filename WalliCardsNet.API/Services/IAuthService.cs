using WalliCardsNet.ClassLibrary.Login;
using WalliCardsNet.ClassLibrary.Register;
using static WalliCardsNet.API.Services.APIAuthService;

namespace WalliCardsNet.API.Services
{
    public interface IAuthService
    {

        Task<RegisterResponseDTO> CreateUserAccountAsync(Guid businessId, string role, string userName, string email);

        Task<LoginResponseDTO> LoginAsync(string email, string password);
        Task<LoginResponseDTO> LoginWithGoogleAsync(string code);
    }
}
