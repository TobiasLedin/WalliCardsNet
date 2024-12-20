
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;
using WalliCardsNet.API.Constants;
using WalliCardsNet.ClassLibrary.Customer;
using WalliCardsNet.API.Services;
using Google.Apis.Walletobjects.v1.Data;
using SendGrid.Helpers.Mail;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomer _customerRepo;
        private readonly IBusiness _businessRepo;
        private readonly IBusinessProfile _profileRepo;
        private readonly IGoogleService _googleService;
        private readonly IGooglePass _googlePassRepository;

        public CustomerController(
            ICustomer customerRepo, 
            IBusiness businessRepo, 
            IBusinessProfile profileRepo, 
            IGoogleService googleService, 
            IGooglePass googlePassRepository)
        {
            _customerRepo = customerRepo;
            _businessRepo = businessRepo;
            _profileRepo = profileRepo;
            _googleService = googleService;
            _googlePassRepository = googlePassRepository;
        }

        
        [HttpGet]
        [Authorize(Policy = Roles.ManagerOrEmployee)]
        public async Task<IActionResult> GetAllByBusinessAsync()
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
                List<CustomerDTO> customersDTO = [];

                foreach (var customer in customers)
                {
                    customersDTO.Add(new CustomerDTO(customer.Id, customer.BusinessId, customer.RegistrationDate, customer.CustomerDetails));
                }

                return Ok(customersDTO);
            }
            else
            {
                return Ok(new List<CustomerDTO>());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var customer = await _customerRepo.GetByIdAsync(id);
            if (customer != null)
            {
                return Ok(new CustomerDTO(customer.Id, customer.BusinessId, customer.RegistrationDate, customer.CustomerDetails));
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddAsync(CustomerDTO customerData)
        {
            try
            {
                var customer = new Customer
                {
                    BusinessId = customerData.BusinessId,
                    CustomerDetails = customerData.CustomerDetails ?? new Dictionary<string, string>()
                };
                await _customerRepo.AddAsync(customer);
                return Created($"api/Customer/{customer.Id}", new CustomerDTO(customer.Id, customer.BusinessId, customer.RegistrationDate, customer.CustomerDetails));
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

                var customerDetails = JsonSerializer.Deserialize<Dictionary<string, string>>(joinFormModel.FormDataJson);

                if (customerDetails != null && customerDetails.TryGetValue("Email", out var email))
                {
                    var customer = new Customer
                    {
                        BusinessId = business.Id,
                        CustomerDetails = customerDetails
                    };
                    await _customerRepo.AddAsync(customer);

                    // GooglePass generation

                    var profile = await _profileRepo.GetActiveByBusinessIdAsync(business.Id);
                    if (profile == null)
                    {
                        return Problem();
                    }

                    var objectCreateResult = await _googleService.CreateGenericObjectAsync(profile, customer); // return JSON instead of GenericObject
                    if (!objectCreateResult.Success || objectCreateResult.Data == null)
                    {
                        return Problem();
                    }

                    var objectJson = JsonSerializer.Serialize(objectCreateResult.Data, _googleService.SerializerOptions());

                    var googlePass = new GooglePass
                    {
                        ObjectId = objectCreateResult.Data.Id,
                        ObjectJson = objectJson,
                        ClassId = profile.GoogleTemplate!.GenericClassId!,
                        ClassJson = profile.GoogleTemplate!.GenericClassJson!,
                        Customer = customer,
                    };

                    var creationResult = await _googleService.CreateSignedJWTAsync(googlePass);
                    if (creationResult.Success)
                    {
                        await _googlePassRepository.AddAsync(googlePass);

                        return Ok(creationResult.Data);
                    }
                    else
                    {
                        return Problem(creationResult.Message);
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(CustomerDTO customerData)
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
        [Authorize(Policy = Roles.ManagerOrEmployee)]
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
