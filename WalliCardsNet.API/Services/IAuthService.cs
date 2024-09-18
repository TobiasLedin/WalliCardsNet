using static WalliCardsNet.API.Services.AuthService;

namespace WalliCardsNet.API.Services
{
    public interface IAuthService
    {

        Task<RegisterResult> RegisterEmployeeAsync(string name, string email, string password);

        Task<LoginResult> LoginAsync(string email, string password);
    }
}
