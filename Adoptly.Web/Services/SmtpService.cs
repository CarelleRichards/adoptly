using Adoptly.Web.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Adoptly.Web.Services;

public class SmtpService
{
    private readonly SmtpConfig _smtpConfig;

    public SmtpService(IOptions<SmtpConfig> smtpConfig) => _smtpConfig = smtpConfig.Value; // Get the Smtp configuration values.

    public async Task SendEmailAsync(string to, string subject, string html)
    {
        // Set up email dependencies and create the email content.

        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_smtpConfig.AdoptlyEmailAddress));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = html };

        // Compose email and dispose of SmtpClient object.

        using var smtp = new SmtpClient();
        
        // Connect to the Smtp server using a secure Tls connection type.
        
        await smtp.ConnectAsync(_smtpConfig.SmtpHost, _smtpConfig.SmtpPort, SecureSocketOptions.StartTls);
        
        // Authenticate the connection and send the email.
        
        await smtp.AuthenticateAsync(_smtpConfig.SmtpUserName, _smtpConfig.SmtpPass);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}