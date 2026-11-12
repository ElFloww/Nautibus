using AmetisAPI.Infrastructure.Services;

namespace AmetisAPI.Infrastructure.Tests.Services
{
    public class AccountEmailTemplatesTests
    {
        // Identity transmet le lien déjà encodé en HTML (HtmlEncoder) : « &amp; » entre les paramètres.
        private const string IdentityLink = "https://localhost:7124/api/v1/auth/confirmEmail?userId=u1&amp;code=AB%2BC";

        [Fact]
        public void Confirmation_Link_Points_To_Site_Page_With_Identity_Parameters()
        {
            AccountEmail v_Email = AccountEmailTemplates.Confirmation(IdentityLink, "https://ametis.exemple.fr/");

            Check.That(v_Email.HtmlBody).Contains("href=\"https://ametis.exemple.fr/confirmation-email?userId=u1&amp;code=AB%2BC\"");
            Check.That(v_Email.HtmlBody).Not.Contains("localhost:7124");
        }

        [Fact]
        public void Confirmation_Keeps_Identity_Link_When_No_Public_Url()
        {
            Check.That(AccountEmailTemplates.Confirmation(IdentityLink, "").HtmlBody).Contains("localhost:7124/api/v1/auth/confirmEmail");
        }

        [Fact]
        public void Reset_Code_Is_Escaped_And_Link_Carries_No_Email()
        {
            AccountEmail v_Email = AccountEmailTemplates.ResetCode("a<b>+/=", "https://ametis.exemple.fr");

            Check.That(v_Email.HtmlBody).Contains("a&lt;b&gt;+/=");
            Check.That(v_Email.HtmlBody).Contains("https://ametis.exemple.fr/mot-de-passe/nouveau?code=a%3Cb%3E%2B%2F%3D");
            Check.That(v_Email.HtmlBody).Not.Contains("email=");
        }
    }
}
