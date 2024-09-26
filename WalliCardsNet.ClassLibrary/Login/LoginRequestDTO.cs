using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WalliCardsNet.ClassLibrary.Login
{
    public record LoginRequestDTO(string Email, string Password);
}
