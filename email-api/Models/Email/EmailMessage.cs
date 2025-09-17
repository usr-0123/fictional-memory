namespace background_api.Models.Email;

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsBodyHtml { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? Priority { get; set; }
}