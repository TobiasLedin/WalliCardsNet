using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WalliCardsNet.ClassLibrary.Login
{
    public record LoginRequest
        (
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email adress")]
        string Email,

        [Required(ErrorMessage = "Password is required")]
        [StringLength(25, MinimumLength = 8)]
        string Password
        );
}
