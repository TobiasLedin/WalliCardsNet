using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.Login;
using WalliCardsNet.ClassLibrary.Register;
using static WalliCardsNet.API.Services.Authentication.AuthService;

namespace WalliCardsNet.API.Services.Authentication
{
    public interface IAuthService
    {
        Task<RegisterResponseDTO> CreateUserAccountAsync(Guid businessId, string role, string userName, string email);
        Task<AuthResult> AuthenticateUserAsync(string email, string password);
        Task<AuthResult> LoginWithGoogleAsync(string code);
    }
}
