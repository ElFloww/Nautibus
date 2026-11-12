namespace AmetisAPI.Domain.Bo.Accounts
{
    public record Favorite(FavoriteKind Kind, string TargetId, DateTime CreatedAtUtc);
}
