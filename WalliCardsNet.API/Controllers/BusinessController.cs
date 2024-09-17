using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Models;
using WalliCardsNet.ClassLibrary;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly IBusiness _businessRepo;
        public BusinessController(IBusiness businessRepo)
        {
            _businessRepo = businessRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _businessRepo.GetAllAsync();
            if (result != null && result.Any())
            {
                List<BusinessDTO> businesses = new List<BusinessDTO>();
                foreach (var business in result)
                {
                    var dto = new BusinessDTO
                    {
                        Id = business.Id,
                        Name = business.Name
                    };
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
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _businessRepo.GetByIdAsync(id);
            if (result != null)
            {
                BusinessDTO dto = new BusinessDTO 
                {
                    Id = result.Id,
                    Name = result.Name
                };
                return Ok(dto);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddAsync(BusinessDTO registerBusinessDTO)
        {
            if (registerBusinessDTO == null)
            {
                return BadRequest();
            }

            Business business = new Business
            {
                Name = registerBusinessDTO.Name,
                PspId = Guid.NewGuid().ToString(),
                CustomerDetailsJson = new List<string> { "Customer 1 detail", "Customer 2 detail", "Customer 3 detail" },
            };

            try
            {
                await _businessRepo.AddAsync(business);
                return Created($"api/Business/{business.Id}", business);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(BusinessDTO businessDTO)
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
        public async Task<IActionResult> RemoveAsync(int id)
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
