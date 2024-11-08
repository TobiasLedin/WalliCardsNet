using WalliCardsNet.ClassLibrary.BusinessProfile;

namespace WalliCardsNet.ClassLibrary.Services
{
    public class ClassLibraryBusinessProfilesService : IClassLibraryBusinessProfilesService
    {
        public BusinessProfileRequestDTO MapResponseDTOToRequestDTO(BusinessProfileResponseDTO businessProfileResponseDTO)
        {
            if (businessProfileResponseDTO != null)
            {
                var businessProfile = new BusinessProfileRequestDTO
                {
                    Id = businessProfileResponseDTO.Id,
                };

                var googlePass = new GooglePassTemplateRequestDTO
                {
                    CardTitle = businessProfileResponseDTO.GooglePassTemplate.CardTitle,
                    Header = businessProfileResponseDTO.GooglePassTemplate.Header,
                    HexBackgroundColor = businessProfileResponseDTO.GooglePassTemplate.HexBackgroundColor,
                    LogoUrl = businessProfileResponseDTO.GooglePassTemplate.LogoUrl,
                    WideLogoUrl = businessProfileResponseDTO.GooglePassTemplate.WideLogoUrl,
                    HeroImage = businessProfileResponseDTO.GooglePassTemplate.HeroImage,
                    FieldsJson = businessProfileResponseDTO.GooglePassTemplate.FieldsJson
                };

                var joinForm = new JoinFormTemplateRequestDTO
                {
                    FieldsJson = businessProfileResponseDTO.JoinForm.FieldsJson,
                    CSSOptionsJson = businessProfileResponseDTO.JoinForm.CSSOptionsJson
                };

                businessProfile.GooglePassTemplate = googlePass;
                businessProfile.JoinFormTemplate = joinForm;
                return businessProfile;
            }
            return null;
        }

        public List<BusinessProfileRequestDTO> MapResponseDTOListToRequestDTOList(List<BusinessProfileResponseDTO> businessProfileResponseDTOs)
        {
            if (businessProfileResponseDTOs != null && businessProfileResponseDTOs.Count > 0)
            {
                var businessProfiles = new List<BusinessProfileRequestDTO>();
                foreach (var businessProfileResponseDTO in businessProfileResponseDTOs)
                {
                    var businessProfile = MapResponseDTOToRequestDTO(businessProfileResponseDTO);
                    businessProfiles.Add(businessProfile);
                }
                return businessProfiles;
            }
            return null;
        }
    }
}
