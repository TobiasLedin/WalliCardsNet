using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.ApplicationUser;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IApplicationUserRepo
    {
        public Task<ApplicationUser> GetByActivationTokenAsync(Guid activationToken);
        public Task SetPasswordAsync(ApplicationUserDTO user);
    }
}
