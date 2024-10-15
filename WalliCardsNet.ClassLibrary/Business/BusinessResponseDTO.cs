using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalliCardsNet.ClassLibrary.Customer;

namespace WalliCardsNet.ClassLibrary.Business
{
    public record BusinessResponseDTO(Guid Id, string UrlToken, string Name, string[] ColumnPreset);
    
}
