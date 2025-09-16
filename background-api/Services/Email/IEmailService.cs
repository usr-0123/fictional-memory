using background_api.Models.Email;

namespace background_api.Services.Email;

public interface IEmailService
{
    Task<bool> SendAsync(EmailMessage emailMessage);
}