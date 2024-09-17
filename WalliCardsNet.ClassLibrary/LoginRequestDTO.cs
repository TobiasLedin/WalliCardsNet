using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WalliCardsNet.ClassLibrary
{
    public record LoginRequestDTO(string Email, string Password);
}
