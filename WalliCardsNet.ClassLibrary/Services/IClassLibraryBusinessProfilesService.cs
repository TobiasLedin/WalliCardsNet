using WalliCardsNet.ClassLibrary.BusinessProfile;

namespace WalliCardsNet.ClassLibrary.Services
{
    public interface IClassLibraryBusinessProfilesService
    {
        public BusinessProfileRequestDTO MapResponseDTOToRequestDTO(BusinessProfileResponseDTO businessProfileResponseDTO);
        public List<BusinessProfileRequestDTO> MapResponseDTOListToRequestDTOList(List<BusinessProfileResponseDTO> businessProfileResponseDTOs);
    }
}
