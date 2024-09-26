using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.Business;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/[controller]")]
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _businessRepo.GetAllAsync();
            if (result != null && result.Any())
            {
                List<BusinessDTO> businesses = new List<BusinessDTO>();
                foreach (var business in result)
                {
                    var dto = new BusinessDTO(business.Id, business.Name);
                    //{
                    //    Id = business.Id,
                    //    Name = business.Name
                    //};
                    businesses.Add(dto);
                }
                return Ok(businesses);
            }
            else
            {
                return Ok(new List<Business>());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _businessRepo.GetByIdAsync(id);
            if (result != null)
            {
                BusinessDTO dto = new BusinessDTO(result.Id, result.Name);
                //{
                //    Id = result.Id,
                //    Name = result.Name
                //};
                return Ok(dto);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Add(BusinessCreateDTO businessData)
        {
            if (businessData == null)
            {
                return BadRequest();
            }

            Business business = new Business
            {
                Id = Guid.NewGuid(),
                Name = businessData.Name,
                PspId = businessData.PspId
            };

            // Add standard field definitions (customer data columns).
            // Standard: <string> email
            business.FieldDefinitions.Add(new FieldDefinition { Id = Guid.NewGuid(), BusinessId = business.Id, FieldName = "email", FieldType = "string", IsRequired = true });

            // Add Manager account tied to business
            var manager = new ApplicationUser
            {
                Email = businessData.ManagerEmail,
                UserName = businessData.ManagerName,
                NormalizedUserName = businessData.ManagerName.ToUpper(),
                BusinessId = business.Id
            };

            try
            {
                await _businessRepo.AddAsync(business);

                // Create new ApplicationUser (manager of the specific business)
                await _userManager.CreateAsync(manager, businessData.ManagerPassword);

                return Created($"api/Business/{business.Id}", new BusinessDTO(business.Id, business.Name));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(BusinessDTO businessDTO)
        {
            if (businessDTO == null)
            {
                return BadRequest();
            }
            var business = await _businessRepo.GetByIdAsync(businessDTO.Id);
            business.Name = businessDTO.Name;
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
        public async Task<IActionResult> Remove(Guid id)
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
