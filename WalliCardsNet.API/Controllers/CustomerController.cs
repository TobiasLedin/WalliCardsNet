using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomer _customerRepo;
        public CustomerController(ICustomer customerRepo)
        {
            _customerRepo = customerRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _customerRepo.GetAllAsync();
            if (result != null && result.Any())
            {
                return Ok(result);
            }
            else
            {
                return Ok(new List<Customer>());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _customerRepo.GetByIdAsync(id);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddAsync(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _customerRepo.AddAsync(customer);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = customer.Id }, customer);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _customerRepo.UpdateAsync(customer);
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
                await _customerRepo.RemoveAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
