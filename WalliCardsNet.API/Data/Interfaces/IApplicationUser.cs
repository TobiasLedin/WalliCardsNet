using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.ApplicationUser;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IApplicationUser
    {
        public Task<ApplicationUser> GetByActivationTokenAsync(Guid activationToken);
        public Task SetPasswordAsync(ApplicationUserDTO user);
    }
}
