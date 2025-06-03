using GradingManagementSystem.Core.Services.Contact;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace GradingManagementSystem.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost = _configuration["EmailSettings:SMTPHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SMTPPort"] ?? string.Empty);
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var emailPassword = _configuration["EmailSettings:EmailPassword"];
            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(fromEmail, emailPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, _configuration["EmailSettings:DisplayName"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
