using System.Text;
using System.Text.Json;
using background_api.Models.Email;
using background_api.Models.RabbitMq;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace background_api.Services.RabbitMq;

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
            exchange: _settings.ExchangeName,
            type: ExchangeType.Direct,
            durable: true);

        _channel.QueueDeclare(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);
            
        _channel.QueueBind(
            queue: _settings.QueueName,
            exchange: _settings.ExchangeName,
            routingKey: _settings.RoutingKey);
            
        _logger.LogInformation("RabbitMQ setup completed - Exchange: {Exchange}, Queue: {Queue}", _settings.ExchangeName, _settings.QueueName);
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
                exchange: _settings.ExchangeName,
                routingKey: _settings.RoutingKey,
                basicProperties: properties,
                body: body);
            
            _logger.LogInformation("Email message published to queue for {To}", emailMessage.To);
            
            await Task.CompletedTask;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to publish email message to queue.");
            throw;
        }
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}