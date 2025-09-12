using api.Models;

namespace api.Services.RabbitMqService
{
    public interface IRabbitMqService
    {
        Task PublishEmailAsync(EmailMessage emailMessage);
        void StartConsuming(Func<EmailMessage, Task> onMessageReceived);
        void Dispose();
    }
};
