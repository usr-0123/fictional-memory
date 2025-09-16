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
    public void StartConsuming(Func<EmailMessage, Task> onMessageReceived)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                var emailMessage = JsonSerializer.Deserialize<EmailMessage>(json);
                
                if (emailMessage == null) return;
                
                await onMessageReceived(emailMessage);
                
                _channel.BasicAck(ea.DeliveryTag, false);
                
                _logger.LogInformation("Email message processed and acknowledged.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing email message: {Message}", json);
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };
        
        _channel.BasicConsume(
            queue: _settings.QueueName,
            autoAck: true,
            consumer: consumer);
        
        _logger.LogInformation("Started consuming message from {Queue}", _settings.QueueName);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}