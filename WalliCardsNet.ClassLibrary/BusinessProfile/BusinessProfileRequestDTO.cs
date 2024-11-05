using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary.BusinessProfile
{
    public class BusinessProfileRequestDTO
    {
        public string LogoUrl { get; set; } = "";
        public string HeroImage { get; set; } = "";
        public string HexBackgroundColor { get; set; } = "#060000";
        public List<List<string>> Fields { get; set; } = new List<List<string>>();
        public GooglePassTemplateDTO GooglePassTemplate { get; set; } = new GooglePassTemplateDTO();
    }
}
