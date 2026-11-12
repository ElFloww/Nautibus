namespace AmetisAPI.Infrastructure.Configuration
{
    /// <summary>
    /// Envoi des e-mails de compte (section « Smtp »). Host vide : aucun envoi, confirmation d'e-mail non exigée.
    /// Le mot de passe est un secret : à fournir hors dépôt (appsettings non versionné ou variable Smtp__Password).
    /// </summary>
    public class SmtpOptions
    {
        public const string Section = "Smtp";

        public string Host { get; set; }
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; } = "Nautibus";

        /// <summary>Adresse publique du site (ex. https://ametis.exemple.fr) : base des liens envoyés par e-mail.</summary>
        public string PublicUrl { get; set; }

        /// <summary>Développement et tests : écrit les e-mails en fichiers .eml dans ce dossier au lieu de les envoyer.</summary>
        public string PickupDirectory { get; set; }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) || !string.IsNullOrWhiteSpace(PickupDirectory);
    }
}
