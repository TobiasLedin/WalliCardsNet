using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary.Register
{
    public record RegisterResponseDTO(bool Success, string? Details, string? userId);

}
