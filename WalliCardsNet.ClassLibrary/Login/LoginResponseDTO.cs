using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary.Login
{
    public record LoginResponseDTO(bool Success, string? Token, string? Details);

}
