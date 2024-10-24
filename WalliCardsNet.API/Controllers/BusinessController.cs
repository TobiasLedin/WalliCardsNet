using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary;
using WalliCardsNet.ClassLibrary.Business;
using WalliCardsNet.API.Constants;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/business")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly IBusiness _businessRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public BusinessController(IBusiness businessRepo, UserManager<ApplicationUser> userManager)
        {
            _businessRepo = businessRepo;
            _userManager = userManager;
        }

        /// <summary>
        /// API GET endpoint that returns list of all businessess in the database.
        /// </summary>
        /// <returns>List<BussinessResponseDTO</returns>
        [HttpGet]
        [Authorize(Roles = Roles.Administrator)]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _businessRepo.GetAllAsync();
            if (result.Count != 0)
            {
                var listBusinessDTO = new List<BusinessResponseDTO>();
                foreach (var business in result)
                {
                    var businessDTO = new BusinessResponseDTO(business.Id, business.UrlToken, business.Name, business.ColumnPreset);

                    listBusinessDTO.Add(businessDTO);
                }
                return Ok(listBusinessDTO);
            }
            else
            {
                return Ok(new List<BusinessResponseDTO>());
            }
        }

        /// <summary>
        /// API GET endpoint that returns a specific business in the database.
        /// </summary>
        /// <returns>BussinessResponseDTO or HTTP status code 404</returns>
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var business = await _businessRepo.GetByIdAsync(id);
            if (business != null)
            {
                var businessDTO = new BusinessResponseDTO(business.Id, business.UrlToken, business.Name, business.ColumnPreset);

                return Ok(businessDTO);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{token}")]
        public async Task<IActionResult> GetByTokenAsync(string token)
        {
            var result = await _businessRepo.GetByTokenAsync(token);
            if (result != null)
            {
                PublicBusinessTokenDTO dto = new PublicBusinessTokenDTO
                {
                    Token = token,
                    Name = result.Name
                };
                return Ok(dto);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("user")]
        [Authorize(Policy = Roles.ManagerOrEmployee)]
        public async Task<IActionResult> GetByUserAsync()
        {
            var idClaim = User.FindFirst("business-id");
            if (idClaim == null)
            {
                return Unauthorized();
            };

            Guid businessId = Guid.Parse(idClaim.Value);

            var business = await _businessRepo.GetByIdAsync(businessId);
            if (business != null)
            {
                var businessDTO = new BusinessResponseDTO(business.Id, business.UrlToken, business.Name, business.ColumnPreset);

                return Ok(businessDTO);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut]
        [Authorize(Roles = Roles.Manager)] //TODO: Look into different endpoints for editing (ColumnPreset = "Employee, Manager" while Name = "Manager" only)!
        public async Task<IActionResult> UpdateAsync(BusinessRequestDTO businessDTO)
        {
            if (businessDTO == null)
            {
                return BadRequest();
            }
            var business = await _businessRepo.GetByIdAsync(businessDTO.Id);

            if (businessDTO.Name != null)
            {
                business.Name = businessDTO.Name;
            }
            if (businessDTO.ColumnPreset != null)
            {
                business.ColumnPreset = businessDTO.ColumnPreset;
            }

            try
            {
                await _businessRepo.UpdateAsync(business);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> RemoveAsync(Guid id)
        {
            try
            {
                await _businessRepo.RemoveAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
