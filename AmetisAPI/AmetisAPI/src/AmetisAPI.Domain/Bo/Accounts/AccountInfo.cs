namespace AmetisAPI.Domain.Bo.Accounts
{
    /// <summary>Informations du compte renvoyées à son seul titulaire.</summary>
    public record AccountInfo(string Email, bool EmailConfirmed);
}
