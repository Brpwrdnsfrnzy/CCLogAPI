using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CCLogAPI.Services;

public interface IEmailService
{
    Task SendAsync(string toAddress, string subject, string htmlBody, string ccAddress = null);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toAddress, string subject, string htmlBody, string ccAddress = null)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromDisplayName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(toAddress));
        if (!string.IsNullOrWhiteSpace(ccAddress))
            message.Cc.Add(MailboxAddress.Parse(ccAddress));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.FromAddress, _settings.FromPassword);
            await client.SendAsync(message);
            _logger.LogInformation("Email sent to {ToAddress}{Cc}: {Subject}",
                toAddress, ccAddress != null ? $" (cc: {ccAddress})" : string.Empty, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToAddress}: {Subject}", toAddress, subject);
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
