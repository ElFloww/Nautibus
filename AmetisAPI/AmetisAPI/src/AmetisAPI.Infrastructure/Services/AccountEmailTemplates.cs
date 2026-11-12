using System.Net;

namespace AmetisAPI.Infrastructure.Services
{
    /// <summary>
    /// Textes des e-mails de compte. Les liens pointent vers les pages du site (PublicUrl) ; l'adresse e-mail
    /// n'est jamais mise dans un lien.
    /// </summary>
    internal static class AccountEmailTemplates
    {
        public const string ConfirmationPath = "/confirmation-email";
        public const string ResetPath = "/mot-de-passe/nouveau";

        /// <summary>Lien de confirmation : la page du site reçoit les paramètres du lien généré par Identity (userId, code).</summary>
        public static AccountEmail Confirmation(string p_IdentityLink, string p_PublicUrl)
        {
            // Identity fournit le lien déjà encodé en HTML : on le décode pour ne pas l'encoder deux fois.
            string v_Link = WebUtility.HtmlDecode(p_IdentityLink);
            if (!string.IsNullOrWhiteSpace(p_PublicUrl) && Uri.TryCreate(v_Link, UriKind.Absolute, out Uri v_Uri))
                v_Link = $"{p_PublicUrl.TrimEnd('/')}{ConfirmationPath}{v_Uri.Query}";
            return new AccountEmail("Confirmez votre adresse e-mail",
                Layout($"<p>Bienvenue sur Nautibus, le tour d’Amiens en temps réel.</p><p>Pour activer votre compte, confirmez votre adresse :</p>{Button(v_Link, "Confirmer mon adresse")}"));
        }

        public static AccountEmail ResetCode(string p_Code, string p_PublicUrl)
        {
            string v_Code = WebUtility.HtmlEncode(p_Code);
            string v_Link = string.IsNullOrWhiteSpace(p_PublicUrl) ? null : $"{p_PublicUrl.TrimEnd('/')}{ResetPath}?code={Uri.EscapeDataString(p_Code)}";
            return new AccountEmail("Réinitialisation de votre mot de passe",
                Layout("<p>Une réinitialisation du mot de passe a été demandée pour votre compte.</p>"
                    + (v_Link is null ? "" : Button(v_Link, "Choisir un nouveau mot de passe"))
                    + $"<p>Code de réinitialisation :</p><p style=\"font-family:monospace;word-break:break-all\">{v_Code}</p>"
                    + "<p>Si vous n'êtes pas à l'origine de cette demande, ignorez cet e-mail : votre mot de passe reste inchangé.</p>"));
        }

        public static AccountEmail ResetLink(string p_Link)
        {
            p_Link = WebUtility.HtmlDecode(p_Link);
            return new AccountEmail("Réinitialisation de votre mot de passe",
                Layout($"<p>Pour choisir un nouveau mot de passe :</p>{Button(p_Link, "Réinitialiser")}<p>Si vous n'êtes pas à l'origine de cette demande, ignorez cet e-mail.</p>"));
        }

        private static string Button(string p_Link, string p_Label) =>
            $"<p><a href=\"{WebUtility.HtmlEncode(p_Link)}\" style=\"background:#00594f;color:#fff;padding:10px 16px;border-radius:8px;text-decoration:none\">{p_Label}</a></p>"
            + $"<p style=\"font-size:12px;color:#64748b\">Lien : {WebUtility.HtmlEncode(p_Link)}</p>";

        private static string Layout(string p_Content) =>
            $"<div style=\"font-family:system-ui,sans-serif;max-width:520px\">{p_Content}<p style=\"font-size:12px;color:#64748b\">Nautibus — application indépendante, non affiliée à Ametis ni à Keolis.</p></div>";
    }
}
