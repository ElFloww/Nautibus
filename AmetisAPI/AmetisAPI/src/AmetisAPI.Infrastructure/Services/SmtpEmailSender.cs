using System.Net;
using System.Net.Mail;
using AmetisAPI.Infrastructure.Configuration;
using AmetisAPI.Infrastructure.Data.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AmetisAPI.Infrastructure.Services
{
    /// <summary>Envoi des e-mails de compte par SMTP (System.Net.Mail). Les adresses ne sont jamais journalisées.</summary>
    internal class SmtpEmailSender(IOptions<SmtpOptions> p_Options, ILogger<SmtpEmailSender> p_Logger) : IEmailSender<AppUser>
    {
        private readonly SmtpOptions m_Options = p_Options.Value;

        public Task SendConfirmationLinkAsync(AppUser p_User, string p_Email, string p_ConfirmationLink)
            => SendAsync(p_Email, AccountEmailTemplates.Confirmation(p_ConfirmationLink, m_Options.PublicUrl), "confirmation");

        public Task SendPasswordResetCodeAsync(AppUser p_User, string p_Email, string p_ResetCode)
            => SendAsync(p_Email, AccountEmailTemplates.ResetCode(p_ResetCode, m_Options.PublicUrl), "réinitialisation (code)");

        public Task SendPasswordResetLinkAsync(AppUser p_User, string p_Email, string p_ResetLink)
            => SendAsync(p_Email, AccountEmailTemplates.ResetLink(p_ResetLink), "réinitialisation (lien)");

        private async Task SendAsync(string p_To, AccountEmail p_Email, string p_Kind)
        {
            using MailMessage v_Message = new()
            {
                From = new MailAddress(m_Options.FromAddress, m_Options.FromName),
                Subject = p_Email.Subject,
                Body = p_Email.HtmlBody,
                IsBodyHtml = true
            };
            v_Message.To.Add(p_To);

            using SmtpClient v_Client = new(m_Options.Host ?? "localhost", m_Options.Port) { EnableSsl = m_Options.EnableSsl };
            if (!string.IsNullOrWhiteSpace(m_Options.PickupDirectory))
            {
                v_Client.EnableSsl = false; // incompatible avec l'écriture en fichiers, aucune donnée ne quitte le poste
                v_Client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                v_Client.PickupDirectoryLocation = m_Options.PickupDirectory;
            }
            else if (!string.IsNullOrWhiteSpace(m_Options.UserName))
            {
                v_Client.Credentials = new NetworkCredential(m_Options.UserName, m_Options.Password);
            }

            await v_Client.SendMailAsync(v_Message);
            p_Logger.LogInformation("E-mail de compte envoyé : {Kind}", p_Kind);
        }
    }
}
