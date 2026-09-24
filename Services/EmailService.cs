using System.Net;
using System.Net.Mail;

namespace FanHubPlus.Services;

/// <summary>
/// SMTP e-mail sender (System.Net.Mail - included in the framework, no extra NuGet).
/// Settings live in appsettings.json under "Smtp".
/// When Smtp:Enabled=false the mail is only written to the console log -
/// perfect for development/demo without a real mail server.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (!_config.GetValue<bool>("Smtp:Enabled"))
        {
            _logger.LogInformation("[EMAIL - SMTP disabled] To: {To} | Subject: {Subject}\n{Body}",
                to, subject, htmlBody);
            return false;
        }

        var host = _config["Smtp:Host"]!;
        var port = _config.GetValue<int>("Smtp:Port", 587);
        var user = _config["Smtp:User"];
        var pass = _config["Smtp:Password"];
        var from = _config["Smtp:From"] ?? "no-reply@fanhubplus.com";

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = _config.GetValue<bool>("Smtp:EnableSsl", true),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        if (!string.IsNullOrEmpty(user))
            client.Credentials = new NetworkCredential(user, pass);

        using var message = new MailMessage(from, to)
        {
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        await client.SendMailAsync(message, ct);
        _logger.LogInformation("E-mail sent to {To}: {Subject}", to, subject);
        return true;
    }
}
