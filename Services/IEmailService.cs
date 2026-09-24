namespace FanHubPlus.Services;

public interface IEmailService
{
    // Sends an HTML e-mail. Returns false when SMTP is disabled (development).
    Task<bool> SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}
