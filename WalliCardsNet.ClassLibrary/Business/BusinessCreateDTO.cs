using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary.Business
{
    public record BusinessCreateDTO(string Name, string PspId, string ManagerEmail, string ManagerName, string ManagerPassword);
    
}
