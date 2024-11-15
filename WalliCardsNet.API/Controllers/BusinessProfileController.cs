using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WalliCardsNet.API.Constants;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.ClassLibrary.BusinessProfile;
using WalliCardsNet.API.Services.Mappers;
using WalliCardsNet.API.Services.GoogleServices.GoogleWallet;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/businessprofile")]
    [ApiController]
    public class BusinessProfileController : ControllerBase
    {
        private readonly IGoogleWallet _googleWalletService;
        private readonly IAPIBusinessProfilesService _businessProfilesService;
        private readonly IBusinessProfileRepo _profileRepo;
        private readonly IBusinessRepo _businessRepo;
        private readonly IGooglePassRepo _googlePassRepo;

        public BusinessProfileController(IGoogleWallet googleWalletService, IAPIBusinessProfilesService businessProfilesService, IBusinessProfileRepo profileRepo, IBusinessRepo businessRepo, IGooglePassRepo googlePassRepo)
        {
            _googleWalletService = googleWalletService;
            _businessProfilesService = businessProfilesService;
            _profileRepo = profileRepo;
            _businessRepo = businessRepo;
            _googlePassRepo = googlePassRepo;
        }


        [HttpGet("all")]
        [Authorize(Policy = Roles.ManagerOrEmployee)]
        public async Task<IActionResult> GetAllForBusinessAsync()
        {
            var businessIdClaim = User.FindFirst("business-id");
            if (businessIdClaim == null)
            {
                return Unauthorized();
            };
            var businessId = businessIdClaim.Value;
            var businessProfiles = await _profileRepo.GetAllAsync(new Guid(businessId));
            if (businessProfiles != null)
            {
                var businessProfileDTOs = _businessProfilesService.MapBusinessProfileListToResponseDTO(businessProfiles);
                return Ok(businessProfileDTOs);
            }
            return NotFound();
        }

        [HttpGet("{token}")]
        public async Task<IActionResult> GetByTokenAsync(string token)
        {
            var business = await _businessRepo.GetByTokenAsync(token);
            var result = await _profileRepo.GetActiveByBusinessIdAsync(business.Id);
            if (result != null)
            {
                var businessProfileRequestDTO = _businessProfilesService.MapBusinessProfileToResponseDTO(result);
                return Ok(businessProfileRequestDTO);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("create")]
        [Authorize(Policy = Roles.ManagerOrEmployee)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddBusinessProfileAsync(BusinessProfileRequestDTO businessProfileRequest)
        {
            var businessIdClaim = User.FindFirst("business-id");
            if (businessIdClaim == null)
            {
                return Unauthorized();
            };

            var businessId = new Guid(businessIdClaim.Value);
            if (businessProfileRequest != null)
            {
                var businessProfile = _businessProfilesService.MapRequestDTOtoBusinessProfile(businessProfileRequest, businessId);
                await _profileRepo.AddAsync(businessProfile);
                await _businessRepo.AddCardDesignFieldsToColumnPresetAsync(businessProfile.JoinForm.FieldsJson, businessId);
                return Created($"api/businessprofile/{businessProfile.Id}", businessProfile);
            }
            return BadRequest();
        }

        //TODO: Fix update/refresh of issued Passes
        [HttpPut]
        [Authorize(Policy = Roles.ManagerOrEmployee)]
        public async Task<IActionResult> UpdateAsync(BusinessProfileRequestDTO businessProfileRequest)
        {
            var businessIdClaim = User.FindFirst("business-id");
            if (businessIdClaim == null)
            {
                return Unauthorized();
            };

            var businessId = new Guid(businessIdClaim.Value);
            if (businessProfileRequest != null)
            {
                await _profileRepo.UpdateAsync(businessProfileRequest, businessId);
                await _businessRepo.AddCardDesignFieldsToColumnPresetAsync(businessProfileRequest.JoinFormTemplate.FieldsJson, businessId);
                return Ok(businessProfileRequest);
            }
            return BadRequest();
        }

        
        [HttpPut("set-active/{id}")]
        [Authorize(Roles = Roles.Manager)]
        public async Task<IActionResult> SetActiveBusinessProfile(Guid id)
        {
            var businessProfile = await _profileRepo.GetByIdAsync(id);
            if (businessProfile == null)
            {
                return NotFound();
            }

            // Trigger GenericClass creation/update (GoogleWallet API). If ClassId exists, update will be performed.
            var createResult = await _googleWalletService.CreateOrUpdateGenericClassAsync(businessProfile);

            if (createResult.Success && createResult.Data != null)
            {
                var genericClassJson = JsonSerializer.Serialize(createResult.Data);

                // Set GoogleTemplate props
                businessProfile.GoogleTemplate!.GenericClassJson = genericClassJson;
                businessProfile.GoogleTemplate!.GenericClassId = createResult.Data.Id;

                // Trigger update of related GenericObjects
                var passList = await _googlePassRepo.GetAllByClassIdAsync(createResult.Data.Id); //TODO: ClassId tied to Profile. Swapping profile will not update passes issues tied to another profile!

                if (passList.Count > 0)
                {
                    var failedUpdates = new List<string>();

                    foreach (var pass in passList)
                    {
                        var updateResult = await _googleWalletService.UpdateGenericObjectAsync(businessProfile, pass.Customer);

                        if (!createResult.Success)
                        {
                            failedUpdates.Add(updateResult.Message);
                        }
                    }

                    if (failedUpdates.Count > 0)
                    {
                        return Problem($"{failedUpdates} out of {passList.Count} passes realated to classId: {createResult.Data.Id} failed to be updated");
                    }
                }
            }
            else
            {
                return Problem("Failed to create/update GenericClass");
            }

            await _profileRepo.SetActiveAsync(id);

            return Ok($"ClassId: {createResult.Data.Id} active");
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Roles.ManagerOrEmployee)]
        public async Task<IActionResult> RemoveAsync(Guid id)
        {
            try
            {
                var profile = await _profileRepo.GetByIdAsync(id);

                if (profile != null && !profile.IsActive)
                {
                    await _profileRepo.RemoveAsync(id);
                    return NoContent();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
