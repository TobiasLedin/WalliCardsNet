using Microsoft.AspNetCore.Identity;

namespace WalliCardsNet.API.Data.Models
{
    // Custom user class extending IdentityUser
    public class ApplicationUser : IdentityUser
    {
        public Business? Business { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    }
}
