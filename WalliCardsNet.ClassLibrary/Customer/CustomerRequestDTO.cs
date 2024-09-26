using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary.Customer
{
    public class CustomerRequestDTO
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Dictionary<string, object>? CustomerDetails { get; set; }

    }
}
