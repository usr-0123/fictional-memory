using api.Models;
using api.Services.RabbitMqService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace api.Services.EmailService
{
    public class EmailQueueProcessor : BackgroundService
    {
        private readonly IRabbitMqService _rabbitMqService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailQueueProcessor> _logger;

        public EmailQueueProcessor(
            IRabbitMqService rabbitMqService, 
            IServiceProvider serviceProvider,
            IEmailService emailService,
            ILogger<EmailQueueProcessor> logger)
        {
            _rabbitMqService = rabbitMqService;
            _serviceProvider = serviceProvider;
            _emailService = emailService;
            _logger = logger;
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Queue Processor started.");
            
            _rabbitMqService.StartConsuming(ProcessEmailMessage);
            
            return Task.CompletedTask;
        }

        private async Task ProcessEmailMessage(EmailMessage emailMessage)
        {
            using var scope = _serviceProvider.CreateScope();

            _logger.LogInformation("Processing email for {To} with subject '{Subject}'", emailMessage.To, emailMessage.Subject);

            var success = await _emailService.SendEmailAsync(emailMessage);

            if (success)
            {
                _logger.LogInformation("Successfully processed email for {To}", emailMessage.To);
            }
            else
            {
                _logger.LogInformation("Failed to process email for {To}", emailMessage.To);
            }
        }

        public override void Dispose()
        {
            _rabbitMqService?.Dispose();
            base.Dispose();
        }
    }
}
