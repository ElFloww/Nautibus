using Microsoft.AspNetCore.Identity;

namespace AmetisAPI.Infrastructure.Services
{
    /// <summary>Messages d'erreur d'Identity en français (interface en français). Les codes restent ceux d'Identity.</summary>
    public class FrenchIdentityErrorDescriber : IdentityErrorDescriber
    {
        private static IdentityError Error(string p_Code, string p_Description) => new() { Code = p_Code, Description = p_Description };

        public override IdentityError DefaultError() => Error(nameof(DefaultError), "Une erreur est survenue.");
        public override IdentityError PasswordMismatch() => Error(nameof(PasswordMismatch), "Mot de passe incorrect.");
        public override IdentityError InvalidToken() => Error(nameof(InvalidToken), "Code invalide ou expiré.");
        public override IdentityError InvalidEmail(string email) => Error(nameof(InvalidEmail), "Adresse e-mail invalide.");
        public override IdentityError InvalidUserName(string userName) => Error(nameof(InvalidUserName), "Identifiant invalide.");
        // Message identique pour un e-mail déjà pris ou non : ne révèle pas quels comptes existent au-delà de ce qu'Identity expose déjà.
        public override IdentityError DuplicateEmail(string email) => Error(nameof(DuplicateEmail), "Cette adresse e-mail est déjà utilisée.");
        public override IdentityError DuplicateUserName(string userName) => Error(nameof(DuplicateUserName), "Cette adresse e-mail est déjà utilisée.");
        public override IdentityError PasswordTooShort(int length) => Error(nameof(PasswordTooShort), $"Le mot de passe doit contenir au moins {length} caractères.");
        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Error(nameof(PasswordRequiresUniqueChars), $"Le mot de passe doit contenir au moins {uniqueChars} caractères différents.");
        public override IdentityError PasswordRequiresNonAlphanumeric() => Error(nameof(PasswordRequiresNonAlphanumeric), "Le mot de passe doit contenir au moins un caractère spécial.");
        public override IdentityError PasswordRequiresDigit() => Error(nameof(PasswordRequiresDigit), "Le mot de passe doit contenir au moins un chiffre.");
        public override IdentityError PasswordRequiresLower() => Error(nameof(PasswordRequiresLower), "Le mot de passe doit contenir au moins une minuscule.");
        public override IdentityError PasswordRequiresUpper() => Error(nameof(PasswordRequiresUpper), "Le mot de passe doit contenir au moins une majuscule.");
        public override IdentityError UserLockoutNotEnabled() => Error(nameof(UserLockoutNotEnabled), "Le verrouillage n'est pas activé pour ce compte.");
        public override IdentityError ConcurrencyFailure() => Error(nameof(ConcurrencyFailure), "Le compte a été modifié entre-temps, réessayez.");
    }
}
