using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary.BusinessProfile
{
    public class JoinFormTemplateResponseDTO
    {
        public string Title { get; set; } = "";
        public string LogoUrl { get; set; } = "";
        public string WideLogoUrl { get; set; } = "";
        public bool UseWideLogo { get; set; }
        public string HeroImageUrl { get; set; } = "";
        public string FieldsJson { get; set; } = "";
        public string CSSOptionsJson { get; set; } = "";
    }
}
