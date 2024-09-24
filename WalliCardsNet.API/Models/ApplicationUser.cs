using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalliCardsNet.API.Models
{
    // Custom user class extending IdentityUser
    public class ApplicationUser : IdentityUser
    {
        [ForeignKey(nameof(Business))]
        public Guid BusinessId { get; set; }
        public Business? Business { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    }
}
