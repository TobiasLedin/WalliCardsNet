using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.BusinessProfile;

namespace WalliCardsNet.API.Services
{
    public interface IAPIBusinessProfilesService
    {
        public BusinessProfileResponseDTO MapBusinessProfileToResponseDTO(BusinessProfile businessProfile);
        public List<BusinessProfileResponseDTO> MapBusinessProfileListToResponseDTO(List<BusinessProfile> businessProfiles);
        BusinessProfile MapRequestDTOtoBusinessProfile (BusinessProfileRequestDTO businessProfileRequestDTO, Guid businessId);
        List<BusinessProfile> MapRequestDTOListToBusinessProfiles (List<BusinessProfileRequestDTO> businessProfileRequestDTOs, Guid businessId);
    }
}
