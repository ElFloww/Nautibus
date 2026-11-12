using AmetisAPI.Infrastructure.Configuration;
using AmetisAPI.Infrastructure.Data.EntityFramework.Entities;
using AmetisAPI.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AmetisAPI.Infrastructure.Tests.Services
{
    public class SmtpEmailSenderTests : IDisposable
    {
        private readonly string m_Directory = Directory.CreateTempSubdirectory("ametis-mail-").FullName;

        public void Dispose()
        {
            Directory.Delete(m_Directory, true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task Reset_Code_Mail_Is_Written_To_Pickup_Directory_Without_Network()
        {
            SmtpEmailSender v_Sender = new(Options.Create(new SmtpOptions
            {
                Host = "smtp.invalid",
                FromAddress = "noreply@ametis.test",
                PublicUrl = "https://ametis.test",
                PickupDirectory = m_Directory
            }), NullLogger<SmtpEmailSender>.Instance);

            await v_Sender.SendPasswordResetCodeAsync(new AppUser(), "usager@exemple.test", "CODE123");

            string v_Mail = await File.ReadAllTextAsync(Directory.GetFiles(m_Directory, "*.eml").Single());
            Check.That(v_Mail).Contains("To: usager@exemple.test").And.Contains("From: \"Nautibus\" <noreply@ametis.test>");
        }
    }
}
