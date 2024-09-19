using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary
{
    public record RegisterRequestDTO(string FormDataId, string UserName, string Password);
    
}
