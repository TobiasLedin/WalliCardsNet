using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.CodeDom;
using System.Text.Json;
using WalliCardsNet.API.Constants;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;
using WalliCardsNet.API.Services;
using WalliCardsNet.ClassLibrary.Business;
using WalliCardsNet.ClassLibrary.BusinessProfile;
using WalliCardsNet.ClassLibrary.Services;
using WalliCardsNet.ClassLibrary.Card;
using WalliCardsNet.ClassLibrary.Customer;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/businessprofile")]
    [ApiController]
    public class BusinessProfileController : ControllerBase
    {
        private readonly IBusinessProfile _businessProfileRepo;
        private readonly ICardTemplate _cardTemplateRepo;
        private readonly IBusiness _businessRepo;
        private readonly IGoogleService _googleService;
        private readonly IBusinessProfilesService _businessProfilesService;

        public BusinessProfileController(ICardTemplate cardTemplateRepo, IBusiness businessRepo, IGoogleService googleService, IBusinessProfile businessProfileRepo, IBusinessProfilesService businessProfilesService)
        {
            _cardTemplateRepo = cardTemplateRepo;
            _businessRepo = businessRepo;
            _googleService = googleService;
            _businessProfileRepo = businessProfileRepo;
            _businessProfilesService = businessProfilesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _cardTemplateRepo.GetAllAsync();
            if (result != null && result.Any())
            {
                return Ok(result);
            }
            else
            {
                return Ok(new List<CardTemplate>());
            }
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
            var businessProfiles = await _businessProfileRepo.GetAllAsync(new Guid(businessId));
            if (businessProfiles != null)
            {
                var businessProfileDTOs = _businessProfilesService.MapBusinessProfileListToResponseDTO(businessProfiles);
                return Ok(businessProfileDTOs);
            }
            return NotFound();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _cardTemplateRepo.GetByIdAsync(id);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{token}")]
        public async Task<IActionResult> GetByTokenAsync(string token)
        {
            var business = await _businessRepo.GetByTokenAsync(token);
            var result = await _cardTemplateRepo.GetByBusinessIdAsync(business.Id);
            if (result != null)
            {
                var cardResponseDTO = new CardResponseDTO
                {
                    DesignJson = result.DesignJson
                };
                return Ok(cardResponseDTO);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Authorize(Policy = Roles.ManagerOrEmployee)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddAsync(CardRequestDTO cardRequestDTO)
        {
            //try
            //{
            //    var business = await _businessRepo.GetByTokenAsync(cardRequestDTO.BusinessToken);
            //    var cardTemplate = new CardTemplate
            //    {
            //        Business = business,
            //        DesignJson = cardRequestDTO.DesignJson
            //    };
            //    await _cardTemplateRepo.AddAsync(cardTemplate);
            //    await _businessRepo.AddCardDesignFieldsToColumnPresetAsync(cardTemplate.DesignJson, business.Id);
            //    return Created($"api/CardTemplate/{cardTemplate.Id}", cardTemplate);
            //}

            //
            // Test av Google Service calls
            //
            var businessIdClaim = User.FindFirst("business-id");
            if (businessIdClaim == null)
            {
                return Unauthorized();
            };

            var businessId = businessIdClaim.Value;

            try
            {
                var business = await _businessRepo.GetByTokenAsync(cardRequestDTO.BusinessToken);
                var cardTemplate = new CardTemplate
                {
                    Business = business,
                    DesignJson = cardRequestDTO.DesignJson
                };
                await _cardTemplateRepo.AddAsync(cardTemplate);
                await _businessRepo.AddCardDesignFieldsToColumnPresetAsync(cardTemplate.DesignJson, business.Id);
                return Created($"api/businessprofile/{cardTemplate.Id}", cardTemplate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
                await _businessProfileRepo.AddAsync(businessProfile);
                return Created($"api/businessprofile/{businessProfile.Id}", businessProfile);
            }
            return BadRequest();
        }

        [HttpPut]
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
                await _businessProfileRepo.UpdateAsync(businessProfileRequest, businessId);
                return Ok(businessProfileRequest);
            }
            return BadRequest();
        }

        // TODO: Authorization => Manager only?
        [HttpPut("set-active/{id}")]
        public async Task<IActionResult> SetActiveBusinessProfile(Guid id)
        {
            var businessProfile = await _businessProfileRepo.GetByIdAsync(id);
            if (businessProfile == null)
            {
                return NotFound();
            }

            // Trigger GenericClass creation/update (GoogleWallet API) + trigger update of related GenericObjects

            await _businessProfileRepo.SetActiveAsync(id);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveAsync(int id)
        {
            try
            {
                await _cardTemplateRepo.RemoveAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
