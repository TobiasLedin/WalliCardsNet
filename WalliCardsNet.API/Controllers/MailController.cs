using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using WalliCardsNet.API.Constants;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/mail")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailService;
        private readonly IAuthService _authService;
        public MailController(IMailService mailService, IAuthService authService)
        {
            _mailService = mailService;
            _authService = authService;
        }

        [HttpPost("invite-employee")]
        public async Task<IActionResult> GetAllAsync(EmailAddress email)
        {
            if (email == null || string.IsNullOrEmpty(email.Email))
            {
                return BadRequest("Invalid email data");
            }

            var to = new EmailAddress(email.Email, email.Name);
            await _mailService.InviteEmployeeEmailAsync(to, "BusinesName");

            return Ok("Email sent successfully");
        }

        [HttpPost("batch-invite")]
        public async Task<IActionResult> BatchInviteAsync(List<string> emailAddresses)
        {
            var businessIdClaim = User.FindFirst("business-id");
            if (businessIdClaim == null || !Guid.TryParse(businessIdClaim.Value, out Guid businessId))
            {
                return BadRequest("Invalid or missing business ID claim.");
            }

            if (emailAddresses == null || emailAddresses.Count == 0)
            {
                return BadRequest("Invalid email data");
            }

            List<EmailAddress> emailAddressesList = new List<EmailAddress>();
            List<EmailAddress> failedEmailAddressesList = new List<EmailAddress>();
            foreach (var emailAddress in emailAddresses)
            {
                var email = new EmailAddress { Email = emailAddress };

                var result = await _authService.CreateUserAccountAsync(businessId, Roles.Employee, emailAddress, emailAddress);
                if (result.Success)
                {
                    emailAddressesList.Add(email);
                }
                else
                {
                    failedEmailAddressesList.Add(email);
                    return BadRequest(failedEmailAddressesList);
                }
            }

            await _mailService.BatchInviteAsync(emailAddressesList);
            return Ok(emailAddresses);
        }

        [HttpPost("send-activation-link")]
        public async Task<IActionResult> SendActivationLinkAsync(EmailAddress email, string applicationUserId)
        {
            if (email == null || string.IsNullOrEmpty(email.Email))
            {
                return BadRequest("Invalid email data");
            }

            var to = new EmailAddress(email.Email, email.Name);
            await _mailService.SendActivationLinkAsync(to, applicationUserId);
            return Ok("Email sent successfully");
        }
    }
}
