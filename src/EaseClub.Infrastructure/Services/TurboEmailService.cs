using EaseClub.Domain.Common.Results;
using EaseClub.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using EaseClub.Domain.Common;
using EaseClub.Application.Common.Interfaces;


namespace EaseClub.Infrastructure.Services
{
    public class TurboEmailService:IEmailService
    {
        public TurboEmailService(IOptions<EmailSettings> emailSettings, ILogger<TurboEmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<TurboEmailService> _logger;
        public async Task<Result<Success>> SendAsync(string toEmail, string Subject, string Message)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_emailSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = Subject;
            email.Body = new TextPart("plain") { Text = Message };

            try
            {
                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP failure sending mail to {email}.", toEmail);
                return Error.Failure(description: "failure sending mail");
            }

            return Result.Success;
        }
    }
}
