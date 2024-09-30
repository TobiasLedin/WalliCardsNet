using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary.Business
{
    public class BusinessDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string BusinessToken { get; set; }
        //public List<ColumnDefinition>? ColumnDefinitions { get; set; }

    }
    
}
