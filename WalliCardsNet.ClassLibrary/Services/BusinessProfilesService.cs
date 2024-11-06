using WalliCardsNet.ClassLibrary.BusinessProfile;
using WalliCardsNet.ClassLibrary.BusinessProfile.Models;

namespace WalliCardsNet.ClassLibrary.Services
{
    public class BusinessProfilesService : IBusinessProfilesService
    {
        public BusinessProfileResponseDTO MapBusinessProfileToResponseDTO(BusinessProfile.Models.BusinessProfile businessProfile)
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
        public List<BusinessProfileResponseDTO> MapBusinessProfileListToResponseDTO(List<BusinessProfile.Models.BusinessProfile> businessProfiles)
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

        public BusinessProfile.Models.BusinessProfile MapRequestDTOtoBusinessProfile(BusinessProfileRequestDTO businessProfileRequestDTO, Guid businessId)
        {
            if (businessProfileRequestDTO != null && businessId != Guid.Empty)
            {
                var businessProfile = new BusinessProfile.Models.BusinessProfile
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

        public List<BusinessProfile.Models.BusinessProfile> MapRequestDTOListToBusinessProfiles(List<BusinessProfileRequestDTO> businessProfileRequestDTOs, Guid businessId)
        {
            if (businessProfileRequestDTOs != null && businessProfileRequestDTOs.Count > 0)
            {
                var businessProfiles = new List<BusinessProfile.Models.BusinessProfile>();
                foreach (var businessProfileRequestDTO in businessProfileRequestDTOs)
                {
                    var businessProfile = MapRequestDTOtoBusinessProfile(businessProfileRequestDTO, businessId);
                    businessProfiles.Add(businessProfile);
                }
                return businessProfiles;
            }
            return null;
        }

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
                    HexBackgroundColor = businessProfileResponseDTO.GooglePassTemplate.HexBackgroundColor,
                    LogoUrl = businessProfileResponseDTO.GooglePassTemplate.LogoUrl,
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
