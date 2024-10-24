using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary.Business
{
    public class ColumnPreset
    {
        public List<string> VisibleColumns { get; set; } = new List<string>();
        public List<string> HiddenColumns { get; set;} = new List<string>();
    }
}
