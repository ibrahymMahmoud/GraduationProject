using GraduationProject.Helpers;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GraduationProject.Services;

public class MailService(IOptions<GmailSettings> gmailOptions) : IMailService
{
    private readonly GmailSettings _gmailSettings = gmailOptions.Value;
    public async Task<bool> SendEmailAsync(EmailMessage emailMessage)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.Subject = emailMessage.Subject;
        mimeMessage.From.Add(new MailboxAddress(_gmailSettings.SenderName, _gmailSettings.SenderEmail));
        mimeMessage.To.Add(new MailboxAddress("User", emailMessage.To));
        var bodyBuilder = new BodyBuilder
        {
            TextBody = emailMessage.Content
        };
        
        mimeMessage.Body = bodyBuilder.ToMessageBody();
        
        using var emailClient = new SmtpClient();

        await emailClient.ConnectAsync(_gmailSettings.Host,
            _gmailSettings.Port, SecureSocketOptions.StartTls, CancellationToken.None);

        await emailClient.AuthenticateAsync(_gmailSettings.SenderEmail,
            _gmailSettings.Password, CancellationToken.None);

        await emailClient.SendAsync(mimeMessage, CancellationToken.None);

        await emailClient.DisconnectAsync(true);
        
        return true;
    }
}