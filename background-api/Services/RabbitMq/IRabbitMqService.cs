using background_api.Models.Email;

namespace background_api.Services.RabbitMq;

public interface IRabbitMqService
{
    void StartConsuming(Func<EmailMessage, Task> onMessageReceived);
    void Dispose();
}