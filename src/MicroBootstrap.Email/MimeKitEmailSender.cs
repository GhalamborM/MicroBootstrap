using MicroBootstrap.Email.Options;

namespace MicroBootstrap.Email;

public class MimeKitEmailSender : IEmailSender
{
    private readonly EmailOptions _config;
    private readonly ILogger<MimeKitEmailSender> _logger;

    public MimeKitEmailSender(IOptions<EmailOptions> config, ILogger<MimeKitEmailSender> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailObject emailObject)
    {
        try
        {
            var email = new MimeMessage { Sender = MailboxAddress.Parse(emailObject.SenderEmail ?? _config.From) };
            email.To.Add(MailboxAddress.Parse(emailObject.ReceiverEmail));
            email.Subject = emailObject.Subject;
            var builder = new BodyBuilder { HtmlBody = emailObject.MailBody };
            email.Body = builder.ToMessageBody();
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_config.MimeKitOptions.Host, _config.MimeKitOptions.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config.MimeKitOptions.UserName, _config.MimeKitOptions.Password);
            smtp.DeliveryStatusNotificationType = DeliveryStatusNotificationType.Full;
            var response = await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation(
                "Email sent. From: {From}, To: {To}, Subject: {Subject}, Content: {Content}",
                _config.From,
                emailObject.ReceiverEmail,
                emailObject.Subject,
                emailObject.MailBody);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }
}
