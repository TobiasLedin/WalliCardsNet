using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary.Customer
{
    public record CustomerDTO(Guid Id, Guid BusinessId, DateTime? RegistrationDate, Dictionary<string, string>? CustomerDetails);
    
}
