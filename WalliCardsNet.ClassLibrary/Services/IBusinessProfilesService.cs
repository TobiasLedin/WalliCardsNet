using WalliCardsNet.ClassLibrary.BusinessProfile;
using WalliCardsNet.ClassLibrary.BusinessProfile.Models;

namespace WalliCardsNet.ClassLibrary.Services
{
    public interface IBusinessProfilesService
    {
        public BusinessProfileResponseDTO MapBusinessProfileToResponseDTO(BusinessProfile.Models.BusinessProfile businessProfile);
        public List<BusinessProfileResponseDTO> MapBusinessProfileListToResponseDTO(List<BusinessProfile.Models.BusinessProfile> businessProfiles);
        public BusinessProfile.Models.BusinessProfile MapRequestDTOtoBusinessProfile (BusinessProfileRequestDTO businessProfileRequestDTO, Guid businessId);
        public List<BusinessProfile.Models.BusinessProfile> MapRequestDTOListToBusinessProfiles (List<BusinessProfileRequestDTO> businessProfileRequestDTOs, Guid businessId);
        public BusinessProfileRequestDTO MapResponseDTOToRequestDTO(BusinessProfileResponseDTO businessProfileResponseDTO);
        public List<BusinessProfileRequestDTO> MapResponseDTOListToRequestDTOList(List<BusinessProfileResponseDTO> businessProfileResponseDTOs);
    }
}
