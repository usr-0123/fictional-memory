using System.Net;
using System.Net.Mail;
using api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace api.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(EmailMessage emailMessage)
        {
            try
            {
                using var client = CreateSmtpClient();
                using var message = CreateMailMessage(emailMessage);
                
                await client.SendMailAsync(message);
                
                _logger.LogInformation("Email sent successfully to {To}", emailMessage.To);
                
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send email to {To}", emailMessage.To);
                return false;
            }
        }

        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Timeout = _emailSettings.Timeout,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password)
            };
        }

        private MailMessage CreateMailMessage(EmailMessage emailMessage)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = emailMessage.Subject,
                Body = emailMessage.Body,
                IsBodyHtml = emailMessage.IsBodyHtml
            };
            
            message.To.Add(emailMessage.To);
            
            if (!string.IsNullOrEmpty(emailMessage.Cc))
                message.CC.Add(emailMessage.Cc);
            
            if (!string.IsNullOrEmpty(emailMessage.Bcc))
                message.Bcc.Add(emailMessage.Bcc);
            
            if (emailMessage.Priority?.ToLower() == "high")
                message.Priority = MailPriority.High;
            
            else if (emailMessage.Priority?.ToLower() == "low")
                message.Priority = MailPriority.Low;

            if (emailMessage.Attachments?.Any() == true)
            {
                foreach (var attachment in emailMessage.Attachments)
                {
                    var stream = new MemoryStream(attachment.Content);
                    
                    message.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
                }
            }

            return message;
        }
    }
}