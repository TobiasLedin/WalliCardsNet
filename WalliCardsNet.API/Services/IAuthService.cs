using static WalliCardsNet.API.Services.AuthService;

namespace WalliCardsNet.API.Services
{
    public interface IAuthService
    {

        Task<RegisterResult> Register(string name, string email, string password);

        Task<LoginResult> Login(string email, string password);
    }
}
