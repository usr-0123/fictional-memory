using background_api.Models.Email;

namespace background_api.Services.RabbitMq;

public interface IRabbitMqService
{
    Task PublishEmailAsync(EmailMessage emailMessage);
    void Dispose();
}