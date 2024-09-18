using SendGrid;
using SendGrid.Helpers.Mail;

namespace WalliCardsNet.API.Services
{
    public class MailService : IMailService
    {
        private readonly SendGridClient _client;
        private readonly EmailAddress _emailSender;

        public MailService()
        {
            var sendGridKey = Environment.GetEnvironmentVariable("SENDGRID-KEY");
            var senderEmail = Environment.GetEnvironmentVariable("EMAIL-SENDER");

            _emailSender = new EmailAddress(senderEmail, "WalliCards");

            _client = new SendGridClient(sendGridKey);
        }

        public async Task SendEmailAsync(EmailAddress to, string subject, string plainTextContent, string htmlContent)
        {
            var msg = MailHelper.CreateSingleEmail(_emailSender, to, subject, htmlContent, plainTextContent);
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
    }
}
