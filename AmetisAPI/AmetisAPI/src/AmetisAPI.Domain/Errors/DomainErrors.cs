using System.Diagnostics.CodeAnalysis;

namespace AmetisAPI.Domain.Errors
{
    [ExcludeFromCodeCoverage(Justification = "It's just constants")]
    public static class DomainErrors
    {
        public const string UnknownDataReplacement = "N/C";


        private const string DOMAIN_PREFIX = "ametis.";

        public const string UnknowError = DOMAIN_PREFIX + "unknown_error";

        public static class Data
        {
            private const string DATA_PREFIX = DOMAIN_PREFIX + "data.";

            public const string NotFound = DATA_PREFIX + GeneralErrors.NotFound;
            public const string Duplicated = DATA_PREFIX + GeneralErrors.Duplicated;
            public const string Unauthorized = DATA_PREFIX + "unauthorized";
            public const string BadRequest = DATA_PREFIX + "badrequest";
            public const string FormatError = DATA_PREFIX + GeneralErrors.InvalidFormat.InvalidParse;
            public const string InvalidNullId = DATA_PREFIX + GeneralErrors.InvalidFormat.InvalidNullId;
        }

        public static class Gtfs
        {
            private const string GTFS_PREFIX = DOMAIN_PREFIX + "gtfs.";

            /// <summary>Téléchargement du fichier GTFS impossible.</summary>
            public const string DownloadFailed = GTFS_PREFIX + "download_failed";

            /// <summary>Fichier GTFS structurellement invalide ou trop dégradé.</summary>
            public const string InvalidFeed = GTFS_PREFIX + "invalid_feed";

            /// <summary>Échec technique pendant l'écriture ou l'activation.</summary>
            public const string ImportFailed = GTFS_PREFIX + "import_failed";
        }

        public static class Push
        {
            /// <summary>Clés VAPID absentes : notifications push indisponibles.</summary>
            public const string Disabled = DOMAIN_PREFIX + "push.disabled";
        }

        internal static class GeneralErrors
        {
            public const string Empty = "empty";
            public const string NotFound = "not_found";
            public const string Duplicated = "duplicated";

            public static class InvalidFormat
            {
                public const string InvalidParse = "parse_error";
                public const string InvalidNullId = "null_id";
                public const string InvalidEmail = "invalid_email";
                public const string InvalidUri = "invalid_uri";
                public const string InvalidLength = "invalid_length";
                public const string InvalidUppercase = "invalid_uppercase";
                public const string InvalidLowercase = "invalid_lowercase";
                public const string InvalidNumber = "invalid_number";
                public const string InvalidRange = "invalid_range";
            }
        }
    }
}