using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.ClassLibrary.ApplicationUser;

namespace WalliCardsNet.API.Controllers
{

    [Route("api/user")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        private readonly IApplicationUser _userRepo;
        public ApplicationUserController(IApplicationUser userRepo)
        {
            _userRepo = userRepo;
        }


        [HttpGet("{activationToken}")]
        public async Task<IActionResult> GetByActivationToken(Guid activationToken)
        {
            var result = await _userRepo.GetByActivationTokenAsync(activationToken);
            if (result != null)
            {
                ApplicationUserDTO dto = new ApplicationUserDTO
                {
                    Id = result.Id,
                };
                return Ok(dto);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut("set-password")]
        public async Task<IActionResult> SetPasswordAsync(ApplicationUserDTO user)
        {
            try
            {
                await _userRepo.SetPasswordAsync(user);
                return Ok(new { Message = "Password updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while setting the password.", Details = ex.Message });
            }
        }
    }
}
