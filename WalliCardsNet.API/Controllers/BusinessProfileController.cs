﻿using Microsoft.AspNetCore.Authorization;
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
using Google.Apis.Walletobjects.v1.Data;

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
        private readonly IGooglePass _googlePassRepository;
        private readonly IAPIBusinessProfilesService _businessProfilesService;

        public BusinessProfileController(
            ICardTemplate cardTemplateRepo, 
            IBusiness businessRepo, 
            IGoogleService googleService,
            IGooglePass googlePassRepository,
            IBusinessProfile businessProfileRepo, 
            IAPIBusinessProfilesService businessProfilesService)
        {
            _cardTemplateRepo = cardTemplateRepo;
            _businessRepo = businessRepo;
            _googleService = googleService;
            _googlePassRepository = googlePassRepository;
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
            var result = await _businessProfileRepo.GetActiveByBusinessIdAsync(business.Id);
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
                await _businessRepo.AddCardDesignFieldsToColumnPresetAsync(businessProfile.JoinForm.FieldsJson, businessId);
                return Created($"api/businessprofile/{businessProfile.Id}", businessProfile);
            }
            return BadRequest();
        }

        //TODO: Fix update/refresh of issued Passes
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
                await _businessRepo.AddCardDesignFieldsToColumnPresetAsync(businessProfileRequest.JoinFormTemplate.FieldsJson, businessId);
                return Ok(businessProfileRequest);
            }
            return BadRequest();
        }

        
        [HttpPut("set-active/{id}")]
        [Authorize(Roles = Roles.Manager)]
        public async Task<IActionResult> SetActiveBusinessProfile(Guid id)
        {
            var businessProfile = await _businessProfileRepo.GetByIdAsync(id);
            if (businessProfile == null)
            {
                return NotFound();
            }

            // Trigger GenericClass creation/update (GoogleWallet API). If ClassId exists, update will be performed.
            var createResult = await _googleService.CreateGenericClassAsync(businessProfile);

            if (createResult.Success && createResult.Data != null)
            {
                var genericClassJson = JsonSerializer.Serialize(createResult.Data, _googleService.SerializerOptions());

                // Set GoogleTemplate props
                businessProfile.GoogleTemplate!.GenericClassJson = genericClassJson;
                businessProfile.GoogleTemplate!.GenericClassId = createResult.Data.Id;

                // Trigger update of related GenericObjects
                var passList = await _googlePassRepository.GetAllByClassIdAsync(createResult.Data.Id); //TODO: ClassId tied to Profile. Swapping profile will not update passes issues tied to another profile!

                if (passList.Count > 0)
                {
                    var failedUpdates = new List<string>();

                    foreach (var pass in passList)
                    {
                        var updateResult = await _googleService.UpdateGenericObjectAsync(businessProfile, pass.Customer);

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

            await _businessProfileRepo.SetActiveAsync(id);

            return Ok($"ClassId: {createResult.Data.Id} active");
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
