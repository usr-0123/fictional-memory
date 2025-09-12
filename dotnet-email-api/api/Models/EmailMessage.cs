namespace api.Models
{
    public class EmailMessage
    {
        public string To { get; set; } = string.Empty;
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsBodyHtml { get; set; } = true;
        public List<EmailAttachment>? Attachments { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Priority { get; set; } = "Normal";
    }
}

