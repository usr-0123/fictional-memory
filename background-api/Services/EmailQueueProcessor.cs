using background_api.Models.Email;
using background_api.Services.Email;
using background_api.Services.RabbitMq;

namespace background_api.Services;

public class EmailQueueProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailQueueProcessor> _logger;
    private readonly IRabbitMqService _rabbitMqService;
    private readonly IEmailService _emailService;

    public EmailQueueProcessor(IServiceProvider serviceProvider, ILogger<EmailQueueProcessor> logger, IRabbitMqService rabbitMqService, IEmailService emailService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMqService = rabbitMqService;
        _emailService = emailService;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Email Queue processor service...");

        _rabbitMqService.StartConsuming(ProcessEmailMessages);
        
        return Task.CompletedTask;
    }

    private async Task ProcessEmailMessages(EmailMessage arg)
    {
        using var scope = _serviceProvider.CreateScope();
        
        _logger.LogInformation("Processing email for {To} with subject '{Subject}'", arg.To, arg.Subject);
        
        var success = await _emailService.SendAsync(arg);

        if (!success)
        {
            _logger.LogError("Failed to send email to {To}", arg.To);
        }
        
        _logger.LogInformation("Email sent to {To} successfully.", arg.To);
    }

    public override void Dispose()
    {
        _rabbitMqService?.Dispose();
        base.Dispose();
    }
}