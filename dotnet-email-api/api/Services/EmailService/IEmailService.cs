using api.Models;

namespace api.Services.EmailService
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailMessage emailMessage);
    }
}