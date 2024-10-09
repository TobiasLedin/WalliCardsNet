using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary;
using WalliCardsNet.ClassLibrary.Business;
using WalliCardsNet.ClassLibrary.Customer;

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

        /// <summary>
        /// API GET endpoint that returns list of all businessess in the database.
        /// </summary>
        /// <returns>List<BussinessResponseDTO</returns>
        //[HttpGet]
        //[Authorize]
        //public async Task<IActionResult> GetAll()
        //{
        //    var result = await _businessRepo.GetAllAsync();
        //    if (result.Count != 0)
        //    {
        //        var listBusinessDTO = new List<BusinessResponseDTO>();
        //        foreach (var business in result)
        //        {
        //            var dataColumns = new List<DataColumnDTO>();
        //            foreach (var column in business.DataColumns)
        //            {
        //               dataColumns.Add(new DataColumnDTO(column.ColumnName, column.ColumnType, column.IsRequired));
        //            }

        //            var businessDTO = new BusinessResponseDTO(business.Id, business.Name, dataColumns);

        //            listBusinessDTO.Add(businessDTO);
        //        }
        //        return Ok(listBusinessDTO);
        //    }
        //    else
        //    {
        //        return Ok(new List<BusinessResponseDTO>());
        //    }
        //}

        /// <summary>
        /// API GET endpoint that returns a specific business in the database.
        /// </summary>
        /// <returns>BussinessResponseDTO or HTTP status code 404</returns>
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var business = await _businessRepo.GetByIdAsync(id);
            if (business != null)
            {
                var dataColumns = new List<DataColumnDTO>();
                foreach (var column in business.DataColumns)
                {
                    dataColumns.Add(new DataColumnDTO(column.Key, column.Title, column.DataType, column.IsSelected));
                }

                var businessDTO = new BusinessResponseDTO(business.Id, business.UrlToken, business.Name, dataColumns);

                return Ok(businessDTO);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{token}")]
        public async Task<IActionResult> GetByToken(string token)
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

        [HttpPost]
        [Authorize]
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
            // Standard: <string> Email, <string> Name

            List<DataColumn> columns = new()
            {
                new DataColumn { Id = Guid.NewGuid(), BusinessId = business.Id, Key = "Email", Title = "Email", DataType = "string" , IsSelected = true },
                new DataColumn { Id = Guid.NewGuid(), BusinessId = business.Id, Key = "Name", Title = "Full name", DataType = "string" , IsSelected = true }
            };
            business.DataColumns.AddRange(columns);

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

                //return Created($"api/Business/{business.Id}", new BusinessResponseDTO(business.Id, business.Name, columns));  //TODO: Utkommenterat pga avsaknad av DataColumn DTO mappning. Behov?
                return Created();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize]
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
        [Authorize]
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
