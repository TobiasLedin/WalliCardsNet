using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WalliCardsNet.ClassLibrary
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [PasswordPropertyText]
        public required string Password { get; set; }
    }
}
