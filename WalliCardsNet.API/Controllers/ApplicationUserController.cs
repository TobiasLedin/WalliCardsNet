using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;
using WalliCardsNet.API.Services;
using WalliCardsNet.ClassLibrary.ApplicationUser;

namespace WalliCardsNet.API.Controllers
{

    [Route("api/user")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        private readonly IApplicationUser _userRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMailService _mailService;
        public ApplicationUserController(IApplicationUser userRepo, UserManager<ApplicationUser> userManager, IMailService mailService)
        {
            _userRepo = userRepo;
            _userManager = userManager;
            _mailService = mailService;
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var emailAddress = new EmailAddress(user.Email);
                    await _mailService.SendForgotPasswordEmail(emailAddress, user.Id);
                }
            }
            return Ok("If the email is registered, an email was sent.");
        }
    }
}
