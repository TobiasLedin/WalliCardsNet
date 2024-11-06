using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.BusinessProfile;

namespace WalliCardsNet.API.Services
{
    public class BusinessProfilesService : IBusinessProfilesService
    {
        public BusinessProfileResponseDTO MapBusinessProfileToResponseDTO(BusinessProfile businessProfile)
        {
            if (businessProfile != null)
            {
                var businessProfileResponseDTO = new BusinessProfileResponseDTO();
                var googlePassRequestDTO = new GooglePassTemplateResponseDTO
                {
                    LogoUrl = businessProfile.GoogleTemplate.LogoUri,
                    HeroImage = businessProfile.GoogleTemplate.HeroImageUri,
                    HexBackgroundColor = businessProfile.GoogleTemplate.HexBackgroundColor,
                    FieldsJson = businessProfile.GoogleTemplate.FieldsJson
                };
                var joinFormRequestDTO = new JoinFormTemplateResponseDTO
                {
                    FieldsJson = businessProfile.JoinForm.FieldsJson,
                    CSSOptionsJson = businessProfile.JoinForm.CSSOptionsJson
                };
                businessProfileResponseDTO.Id = businessProfile.Id;
                businessProfileResponseDTO.IsActive = businessProfile.IsActive;
                businessProfileResponseDTO.GooglePassTemplate = googlePassRequestDTO;
                businessProfileResponseDTO.JoinForm = joinFormRequestDTO;
                return businessProfileResponseDTO;
            }
            return null;
        }
        public List<BusinessProfileResponseDTO> MapBusinessProfileListToResponseDTO(List<BusinessProfile> businessProfiles)
        {
            if (businessProfiles != null && businessProfiles.Count > 0)
            {
                var responseDTOs = new List<BusinessProfileResponseDTO>();
                foreach (var businessProfile in businessProfiles)
                {
                    var businessProfileResponseDTO = MapBusinessProfileToResponseDTO(businessProfile);
                    responseDTOs.Add(businessProfileResponseDTO);
                }
                return responseDTOs;
            }
            return null;
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
                    HexBackgroundColor = businessProfileRequestDTO.GooglePassTemplate.HexBackgroundColor,
                    LogoUri = businessProfileRequestDTO.GooglePassTemplate.LogoUrl,
                    HeroImageUri = businessProfileRequestDTO.GooglePassTemplate.HeroImage,
                    FieldsJson = businessProfileRequestDTO.GooglePassTemplate.FieldsJson
                };

                var joinForm = new JoinForm
                {
                    BusinessProfileId = businessId,
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
