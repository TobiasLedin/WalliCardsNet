﻿using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Models;
using WalliCardsNet.API.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailService;
        public MailController(IMailService mailService)
        {
            _mailService = mailService;
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
    }
}