
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary;
using WalliCardsNet.ClassLibrary.Customer;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomer _customerRepo;
        private readonly IBusiness _businessRepo;
        public CustomerController(ICustomer customerRepo, IBusiness businessRepo)
        {
            _customerRepo = customerRepo;
            _businessRepo = businessRepo;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var businessIdClaim = User.FindFirst("business-id");
            if (businessIdClaim == null)
            {
                return Unauthorized();
            };

            Guid businessId = Guid.Parse(businessIdClaim.Value);    

            var customers = await _customerRepo.GetAllByBusinessAsync(businessId);
            if (customers.Count != 0)
            {
                List<CustomerResponseDTO> customersDTO = [];

                foreach (var customer in customers)
                {
                    customersDTO.Add(new CustomerResponseDTO(customer.Id, customer.RegistrationDate, customer.CustomerDetails));
                }

                return Ok(customersDTO);
            }
            else
            {
                return Ok(new List<CustomerResponseDTO>());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var customer = await _customerRepo.GetByIdAsync(id);
            if (customer != null)
            {
                return Ok(new CustomerResponseDTO(customer.Id, customer.RegistrationDate, customer.CustomerDetails));
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddAsync(CustomerRequestDTO customerData)
        {
            try
            {
                var customer = new Customer
                {
                    BusinessId = customerData.BusinessId,
                    CustomerDetails = customerData.CustomerDetails
                };
                await _customerRepo.AddAsync(customer);
                return Created($"api/Customer/{customer.Id}", new CustomerResponseDTO(customer.Id, customer.RegistrationDate, customer.CustomerDetails));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("join")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddByJoinFormModelAsync(JoinFormModel joinFormModel)
        {
            try
            {
                var business = await _businessRepo.GetByTokenAsync(joinFormModel.BusinessToken);
                var customer = new Customer
                {
                    BusinessId = business.Id,
                    CustomerDetails = JsonSerializer.Deserialize<Dictionary<string, string>>(joinFormModel.FormDataJson)
                };
                await _customerRepo.AddAsync(customer);
                return Created($"api/Customer/{customer.Id}", new CustomerResponseDTO(customer.Id, customer.RegistrationDate, customer.CustomerDetails));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(CustomerRequestDTO customerData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _customerRepo.GetByIdAsync(customerData.Id);
            if (customer != null && customerData.CustomerDetails != null)
            {
                customer.CustomerDetails = customerData.CustomerDetails;

                try
                {
                    await _customerRepo.UpdateAsync(customer);
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            };

            return NotFound();
  
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveAsync(Guid id)
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
