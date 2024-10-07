using SendGrid;
using SendGrid.Helpers.Mail;
using WalliCardsNet.API.Data.Interfaces;

namespace WalliCardsNet.API.Services
{
    public class MailService : IMailService
    {
        private readonly SendGridClient _client;
        private readonly EmailAddress _emailSender;
        private readonly IActivationToken _activationTokenRepo;

        public MailService(IActivationToken activationTokenRepo)
        {
            var sendGridKey = Environment.GetEnvironmentVariable("SENDGRID-KEY");
            var senderEmail = Environment.GetEnvironmentVariable("EMAIL-SENDER");

            _emailSender = new EmailAddress(senderEmail, "WalliCards");
            _client = new SendGridClient(sendGridKey);
            _activationTokenRepo = _activationTokenRepo;
        }

        public async Task SendEmailAsync(EmailAddress to, string subject, string htmlContent, string plainTextContent)
        {
            var msg = MailHelper.CreateSingleEmail(_emailSender, to, subject, plainTextContent, htmlContent);
            var response = await _client.SendEmailAsync(msg);
            var responseBody = await response.Body.ReadAsStringAsync();
        }

        public async Task InviteEmployeeEmailAsync(EmailAddress to, string businessName)
        {
            var subject = $"You have been invited to join {businessName} on WalliCards!";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                    <head>
                        <meta charset='UTF-8'>
                        <title>Invitation to WalliCards</title>
                    </head>
                    <body>
                        <p>Hello <strong>{to.Name}</strong>!</p>
                        <p>You have been invited by <strong>{businessName}</strong> to join them on WalliCards.</p>
                        <p><a href='#' style='color: blue;'>Click here to join!</a></p>
                    </body>
                </html>";
            var plainTextContent = $"Hello {to.Name}! You have been invited by {businessName} to join them on WalliCards. Click the link to join!";
            await SendEmailAsync(to, subject, htmlContent, plainTextContent);
        }

        public async Task SendActivationLinkAsync(EmailAddress to, string applicationUserId)
        {
            var activationToken = _activationTokenRepo.GetTokenAsync(applicationUserId);
            string activationLink = $"https://localhost/7102/activate/{activationToken.Result.Id}";
            var subject = $"Activate your account";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                    <head>
                        <meta charset='UTF-8'>
                        <title>Invitation to WalliCards</title>
                    </head>
                    <body>
                        <p>Hello!</p>
                        <p><a href='{activationLink}'>Click here</a> to activate your account.</p>
                    </body>
                </html>";
            var plainTextContent = $"Hello! Click here to activate your account.";
            await SendEmailAsync(to, subject, htmlContent, plainTextContent);
        }
    }
}
