using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary.BusinessProfile
{
    public class BusinessProfileRequestDTO
    {
        public GooglePassTemplateRequestDTO GooglePassTemplate { get; set; } = new GooglePassTemplateRequestDTO();
        public JoinFormRequestDTO JoinFormTemplate { get; set; }
    }
}
