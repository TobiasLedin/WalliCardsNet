using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary.BusinessProfile
{
    public class BusinessProfileRequestDTO
    {
        public Guid Id { get; set; }
        public GooglePassTemplateRequestDTO GooglePassTemplate { get; set; } = new GooglePassTemplateRequestDTO();
        public JoinFormTemplateRequestDTO JoinFormTemplate { get; set; } = new JoinFormTemplateRequestDTO();
    }
}
