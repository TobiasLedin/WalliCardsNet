using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary.BusinessProfile
{
    public class BusinessProfileResponseDTO
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }

        public GooglePassTemplateResponseDTO GooglePassTemplate { get; set; } = new GooglePassTemplateResponseDTO();
        public JoinFormTemplateResponseDTO JoinForm { get; set; } = new JoinFormTemplateResponseDTO();
    }
}
