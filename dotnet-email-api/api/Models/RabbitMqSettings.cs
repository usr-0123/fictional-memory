namespace api.Models;

public class RabbitMqSettings
{
    public string HostName { get; set; } = string.Empty;
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string EmailQueueName { get; set; } = string.Empty;
    public string EmailExchangeName { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
}