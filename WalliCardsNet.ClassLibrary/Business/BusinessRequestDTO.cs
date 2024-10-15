using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary.Business
{
    public record BusinessRequestDTO(Guid Id, string? Name, string[]? ColumnPreset);
    
}
