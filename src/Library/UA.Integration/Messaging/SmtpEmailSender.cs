using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using UA.Facade.Interface.Integration;
namespace UA.Integration.Messaging;
public sealed class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendAsync(string destination, string subject, string body, CancellationToken ct = default)
    {
        var host = configuration["Smtp:Host"];
        var from = configuration["Smtp:From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("SMTP is not configured; message was not delivered.");
        using var message = new MailMessage(from, destination, subject, body);
        using var smtp = new SmtpClient(host, int.TryParse(configuration["Smtp:Port"], out int p) ? p : 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(configuration["Smtp:Username"], configuration["Smtp:Password"])
        };
        await smtp.SendMailAsync(message, ct);
    }
}
