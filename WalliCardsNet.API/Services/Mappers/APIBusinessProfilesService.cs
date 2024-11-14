using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.BusinessProfile;

namespace WalliCardsNet.API.Services.Mappers
{
    public class APIBusinessProfilesService : IAPIBusinessProfilesService
    {
        public BusinessProfileResponseDTO MapBusinessProfileToResponseDTO(BusinessProfile businessProfile)
        {
            if (businessProfile != null)
            {
                var businessProfileResponseDTO = new BusinessProfileResponseDTO();
                var googlePassRequestDTO = new GooglePassTemplateResponseDTO
                {
                    CardTitle = businessProfile.GoogleTemplate.CardTitle,
                    Header = businessProfile.GoogleTemplate.Header,
                    LogoUrl = businessProfile.GoogleTemplate.LogoUri,
                    WideLogoUrl = businessProfile.GoogleTemplate.WideLogoUri,
                    HeroImage = businessProfile.GoogleTemplate.HeroImageUri,
                    UseWideLogo = businessProfile.GoogleTemplate.UseWideLogo,
                    HexBackgroundColor = businessProfile.GoogleTemplate.HexBackgroundColor,
                    FieldsJson = businessProfile.GoogleTemplate.FieldsJson
                };
                var joinFormRequestDTO = new JoinFormTemplateResponseDTO
                {
                    Title = businessProfile.JoinForm.Title,
                    LogoUrl = businessProfile.JoinForm.LogoUrl,
                    WideLogoUrl = businessProfile.JoinForm.WideLogoUrl,
                    UseWideLogo = businessProfile.JoinForm.UseWideLogo,
                    HeroImageUrl = businessProfile.JoinForm.HeroImageUrl,
                    FieldsJson = businessProfile.JoinForm.FieldsJson,
                    CSSOptionsJson = businessProfile.JoinForm.CSSOptionsJson
                };
                businessProfileResponseDTO.Id = businessProfile.Id;
                businessProfileResponseDTO.IsActive = businessProfile.IsActive;
                businessProfileResponseDTO.DateCreated = businessProfile.DateCreated;
                businessProfileResponseDTO.GooglePassTemplate = googlePassRequestDTO;
                businessProfileResponseDTO.JoinForm = joinFormRequestDTO;
                return businessProfileResponseDTO;
            }
            return null;
        }
        public List<BusinessProfileResponseDTO> MapBusinessProfileListToResponseDTO(List<BusinessProfile> businessProfiles)
        {
            var responseDTOs = new List<BusinessProfileResponseDTO>();

            if (businessProfiles != null && businessProfiles.Count > 0)
            {
                foreach (var businessProfile in businessProfiles)
                {
                    var businessProfileResponseDTO = MapBusinessProfileToResponseDTO(businessProfile);
                    responseDTOs.Add(businessProfileResponseDTO);
                }
            }
            return responseDTOs;
        }

        public BusinessProfile MapRequestDTOtoBusinessProfile(BusinessProfileRequestDTO businessProfileRequestDTO, Guid businessId)
        {
            if (businessProfileRequestDTO != null && businessId != Guid.Empty)
            {
                var businessProfile = new BusinessProfile
                {
                    BusinessId = businessId,
                    IsActive = false,
                };

                var googlePass = new GooglePassTemplate
                {
                    BusinessProfileId = businessProfile.Id,
                    CardTitle = businessProfileRequestDTO.GooglePassTemplate.CardTitle,
                    Header = businessProfileRequestDTO.GooglePassTemplate.Header,
                    WideLogoUri = businessProfileRequestDTO.GooglePassTemplate.WideLogoUrl,
                    UseWideLogo = businessProfileRequestDTO.GooglePassTemplate.UseWideLogo,
                    HexBackgroundColor = businessProfileRequestDTO.GooglePassTemplate.HexBackgroundColor,
                    LogoUri = businessProfileRequestDTO.GooglePassTemplate.LogoUrl,
                    HeroImageUri = businessProfileRequestDTO.GooglePassTemplate.HeroImage,
                    FieldsJson = businessProfileRequestDTO.GooglePassTemplate.FieldsJson
                };

                var joinForm = new JoinForm
                {
                    BusinessProfileId = businessId,
                    Title = businessProfileRequestDTO.JoinFormTemplate.Title,
                    LogoUrl = businessProfileRequestDTO.JoinFormTemplate.LogoUrl,
                    WideLogoUrl = businessProfileRequestDTO.JoinFormTemplate.WideLogoUrl,
                    HeroImageUrl = businessProfileRequestDTO.JoinFormTemplate.HeroImageUrl,
                    UseWideLogo = businessProfileRequestDTO.JoinFormTemplate.UseWideLogo,
                    FieldsJson = businessProfileRequestDTO.JoinFormTemplate.FieldsJson,
                    CSSOptionsJson = businessProfileRequestDTO.JoinFormTemplate.CSSOptionsJson
                };

                businessProfile.GoogleTemplate = googlePass;
                businessProfile.JoinForm = joinForm;
                return businessProfile;
            }
            return null;
        }

        public List<BusinessProfile> MapRequestDTOListToBusinessProfiles(List<BusinessProfileRequestDTO> businessProfileRequestDTOs, Guid businessId)
        {
            if (businessProfileRequestDTOs != null && businessProfileRequestDTOs.Count > 0)
            {
                var businessProfiles = new List<BusinessProfile>();
                foreach (var businessProfileRequestDTO in businessProfileRequestDTOs)
                {
                    var businessProfile = MapRequestDTOtoBusinessProfile(businessProfileRequestDTO, businessId);
                    businessProfiles.Add(businessProfile);
                }
                return businessProfiles;
            }
            return null;
        }
    }
}
