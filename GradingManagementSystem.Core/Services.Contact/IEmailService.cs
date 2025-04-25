namespace GradingManagementSystem.Core.Services.Contact
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
