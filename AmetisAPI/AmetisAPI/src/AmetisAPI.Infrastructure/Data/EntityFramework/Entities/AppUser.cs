using Microsoft.AspNetCore.Identity;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    /// <summary>Compte utilisateur (ASP.NET Core Identity). Données personnelles : ne jamais journaliser l'e-mail.</summary>
    public class AppUser : IdentityUser;
}
