using System.Net.Mail;
using background_api.Models.Email;
using background_api.Models.RabbitMq;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace background_api.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly SmtpClient _smtpClient;
    private readonly ILogger<EmailService> _logger;
    
    public EmailService(SmtpClient smtpClient, IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _settings = emailSettings.Value;
        _smtpClient = smtpClient;
        _logger = logger;
    }
    
    public async Task<bool> SendAsync(EmailMessage emailMessage)
    {
        try
        {
            using var message = CreateMailMessage(emailMessage);
            
            await _smtpClient.SendMailAsync(message);
            
            _logger.LogInformation($"Email sent successfully to {emailMessage.To}");
            
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to send email to {To}", emailMessage.To);
            
            return false;
        }
    }

    private MailMessage CreateMailMessage(EmailMessage emailMessage)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = emailMessage.Subject,
            Body = emailMessage.Body,
            IsBodyHtml = emailMessage.IsBodyHtml
        };
        
        message.To.Add(emailMessage.To);
        
        if (!string.IsNullOrWhiteSpace(emailMessage.Cc))
            message.CC.Add(emailMessage.Cc);
        
        if (!string.IsNullOrWhiteSpace(emailMessage.Bcc))
            message.Bcc.Add(emailMessage.Bcc);
        
        message.Priority = emailMessage.Priority?.ToLower() switch
        {
            "high" => MailPriority.High,
            "low" => MailPriority.Low,
            _ => MailPriority.Normal,
        };

        if (emailMessage.Attachments?.Count > 0)
        {
            foreach (var attachment in emailMessage.Attachments)
            {
                var stream = new MemoryStream(attachment.Content);
                
                message.Attachments.Add(new Attachment(stream, attachment.ContentType));
            }
        }
        
        return message;
    }
}