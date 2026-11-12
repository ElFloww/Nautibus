namespace AmetisAPI.Infrastructure.Configuration
{
    /// <summary>
    /// Clés VAPID (section « Push ») : générées par « dotnet run --project src/AmetisAPI.API -- generate-vapid-keys ».
    /// La clé privée est un secret : hors dépôt (appsettings non versionné ou variable Push__PrivateKey).
    /// </summary>
    public class PushOptions
    {
        public const string Section = "Push";

        /// <summary>Clé publique (point P-256 non compressé, base64url).</summary>
        public string PublicKey { get; set; }

        /// <summary>Clé privée (scalaire P-256 de 32 octets, base64url).</summary>
        public string PrivateKey { get; set; }

        /// <summary>Contact de l'émetteur exigé par les services push : « mailto:… » ou adresse https du site.</summary>
        public string Subject { get; set; }
    }
}
