﻿using SendGrid;
using SendGrid.Helpers.Mail;

namespace WalliCardsNet.API.Services
{
    public interface IMailService
    {
        public Task SendEmailAsync(EmailAddress to, string subject, string plainTextContent, string htmlContent);
        public Task InviteEmployeeEmailAsync(EmailAddress to, string businessName);
    }
}