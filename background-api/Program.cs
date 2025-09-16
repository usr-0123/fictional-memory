using System.Net;
using System.Net.Mail;
using background_api.Models.Email;
using background_api.Models.RabbitMq;
using background_api.Services;
using background_api.Services.Email;
using background_api.Services.RabbitMq;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

// Configure RabbitMq settings
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQSettings"));

// Configure email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Register services
builder.Services.AddSingleton(provider =>
{
    var opt = provider.GetRequiredService<IOptions<EmailSettings>>();
    
    var settings = opt.Value;

    return new SmtpClient(settings.SmtpServer, settings.SmtpPort)
    {
        EnableSsl = settings.EnableSsl,
        Timeout = settings.Timeout,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(settings.Username, settings.Password)
    };
});

builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddHostedService<EmailQueueProcessor>();

var app = builder.Build();

app.Run();
