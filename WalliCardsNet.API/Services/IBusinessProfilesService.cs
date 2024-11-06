using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.BusinessProfile;

namespace WalliCardsNet.API.Services
{
    public interface IBusinessProfilesService
    {
        public BusinessProfileResponseDTO MapBusinessProfileToResponseDTO(BusinessProfile businessProfile);
        public List<BusinessProfileResponseDTO> MapBusinessProfileListToResponseDTO(List<BusinessProfile> businessProfiles);
        public BusinessProfile MapRequestDTOtoBusinessProfile(BusinessProfileRequestDTO businessProfileRequestDTO, Guid businessId);
        public List<BusinessProfile> MapRequestDTOListToBusinessProfiles(List<BusinessProfileRequestDTO> businessProfileRequestDTOs, Guid businessId);
        public BusinessProfileRequestDTO MapResponseDTOToRequestDTO(BusinessProfileResponseDTO businessProfileResponseDTO);
        public List<BusinessProfileRequestDTO> MapResponseDTOListToRequestDTOList(List<BusinessProfileResponseDTO> businessProfileResponseDTOs);
    }
}
