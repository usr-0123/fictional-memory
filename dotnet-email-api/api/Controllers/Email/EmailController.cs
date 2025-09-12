using api.Models;
using api.Services.RabbitMqService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace api.Controllers.Email
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(IRabbitMqService rabbitMqService, ILogger<EmailController> logger)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailMessage emailMessage)
        {
            try
            {
                if (string.IsNullOrEmpty(emailMessage.To) || string.IsNullOrEmpty(emailMessage.Subject))
                {
                    return BadRequest("To and subject fields are required.");
                }

                await _rabbitMqService.PublishEmailAsync(emailMessage);
                
                _logger.LogInformation("Email queued for delivery to {To}", emailMessage.To);
                
                return Ok(new {message = "Email queued successfully", timestamp = DateTime.UtcNow});
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error queueing email.");
                return StatusCode(500, new { error = "Failed to queue email." });
            }
        }

        [HttpPost("send-bulk")]
        public async Task<IActionResult> SendEmailBulk([FromBody] List<EmailMessage> emailMessages)
        {
            try
            {
                var results = new List<object>();

                foreach (var email in emailMessages)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(email.To) || string.IsNullOrEmpty(email.Subject))
                        {
                            results.Add(new {email = email.To, success = false, error = "Missing required fields."});
                            continue;
                        }
                        
                        await _rabbitMqService.PublishEmailAsync(email);
                        
                        results.Add(new {email = email.To, success = true});
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error queueing email for {To}.", email.To);
                    }
                }
                
                return Ok(new { results, timestamp = DateTime.UtcNow });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing bulk emails");
                return StatusCode(500, new { error = "Failed to process bulk emails." });
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new {status = "healthy", timestamp = DateTime.UtcNow});
        }
    }
}
