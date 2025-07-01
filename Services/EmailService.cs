using MailKit.Net.Smtp;
using MimeKit;

namespace DocumentManagementBackend.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser = "gunjanbhattacharya009@gmail.com";
        private readonly string _smtpPass = "ysrw iabj asen smvb";

        public void SendEmail(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("IMS-DMS", _smtpUser));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };


            using var client = new SmtpClient();
            client.Connect(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate(_smtpUser, _smtpPass);
            client.Send(message);
            client.Disconnect(true);
        }
    }
}