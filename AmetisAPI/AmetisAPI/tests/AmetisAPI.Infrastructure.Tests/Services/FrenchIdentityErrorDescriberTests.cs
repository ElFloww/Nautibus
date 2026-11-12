using AmetisAPI.Infrastructure.Services;

namespace AmetisAPI.Infrastructure.Tests.Services
{
    public class FrenchIdentityErrorDescriberTests
    {
        [Fact]
        public void Password_Rules_Are_Described_In_French_With_Identity_Codes()
        {
            FrenchIdentityErrorDescriber v_Describer = new();

            Check.That(v_Describer.PasswordTooShort(6).Description).IsEqualTo("Le mot de passe doit contenir au moins 6 caractères.");
            Check.That(v_Describer.PasswordTooShort(6).Code).IsEqualTo("PasswordTooShort");
            Check.That(v_Describer.InvalidToken().Description).IsEqualTo("Code invalide ou expiré.");
        }
    }
}
