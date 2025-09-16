using System.Text;
using System.Text.Json;
using api.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace api.Services.RabbitMqService
{
    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqService> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        
        
        public RabbitMqService(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password
            };
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            SetupExchangeAndQueue();
        }

        private void SetupExchangeAndQueue()
        {
            _channel.ExchangeDeclare(
                exchange: _settings.EmailExchangeName,
                type: ExchangeType.Direct,
                durable: true);

            _channel.QueueDeclare(
                queue: _settings.EmailQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);
            
            _channel.QueueBind(
                queue: _settings.EmailQueueName,
                exchange: _settings.EmailExchangeName,
                routingKey: _settings.RoutingKey);
            
            _logger.LogInformation("RabbitMQ setup completed - Exchange: {Exchange}, Queue: {Queue}", _settings.EmailExchangeName, _settings.EmailQueueName);
        }
        
        public async Task PublishEmailAsync(EmailMessage emailMessage)
        {
            try
            {
                var json = JsonSerializer.Serialize(emailMessage);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = _channel.CreateBasicProperties();
                
                properties.Persistent = true;
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                
                _channel.BasicPublish(
                    exchange: _settings.EmailExchangeName,
                    routingKey: _settings.RoutingKey,
                    basicProperties: properties,
                    body: body);
                
                _logger.LogInformation("Email message published to queue for {To}", emailMessage.To);

                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to publish email message to queue");
                throw;
            }
        }

        public void StartConsuming(Func<EmailMessage, Task> onMessageReceived)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                try
                {
                    var emailMessage = JsonSerializer.Deserialize<EmailMessage>(json);

                    if (emailMessage != null)
                    {
                        await onMessageReceived(emailMessage);

                        _channel.BasicAck(ea.DeliveryTag, false);
                        _logger.LogDebug("Email message processed and acknowledged");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error processing email message: {Message}", json);
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            
            _channel.BasicConsume(
                queue: _settings.EmailQueueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Started consuming message from {Queue}", _settings.EmailQueueName);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
