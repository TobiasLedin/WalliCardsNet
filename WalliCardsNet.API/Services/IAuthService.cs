using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.Login;
using WalliCardsNet.ClassLibrary.Register;
using static WalliCardsNet.API.Services.AuthService;

namespace WalliCardsNet.API.Services
{
    public interface IAuthService
    {
        Task<RegisterResponseDTO> CreateUserAccountAsync(Guid businessId, string role, string userName, string email);
        Task<AuthResult> AuthenticateUserAsync(string email, string password);
        Task<AuthResult> LoginWithGoogleAsync(string code);
    }
}
