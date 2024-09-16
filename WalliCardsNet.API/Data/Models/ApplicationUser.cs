using Microsoft.AspNetCore.Identity;

namespace WalliCardsNet.API.Data.Models
{
    // Override of the standard IdentityUser class.
    // Int Id used instead of string Id (Identity standard).

    public class ApplicationUser : IdentityUser<int>
    {

    }
}
