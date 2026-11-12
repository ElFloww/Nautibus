namespace AmetisAPI.Core.Push
{
    /// <summary>
    /// Le serveur envoie des requêtes vers l'adresse fournie par le navigateur : seuls les services push connus sont
    /// acceptés (empêche d'utiliser le serveur pour appeler une adresse interne ou arbitraire).
    /// </summary>
    public static class PushEndpointRules
    {
        private static readonly string[] m_AllowedHostSuffixes =
        [
            "fcm.googleapis.com",            // Chrome, Edge (Chromium), Android
            "push.services.mozilla.com",     // Firefox
            "notify.windows.com",            // Edge (WNS)
            "push.apple.com"                 // Safari, iOS (application installée)
        ];

        public static bool IsAllowed(string p_Endpoint)
        {
            if (!Uri.TryCreate(p_Endpoint, UriKind.Absolute, out Uri v_Uri) || v_Uri.Scheme != Uri.UriSchemeHttps || !v_Uri.IsDefaultPort)
                return false;
            string v_Host = v_Uri.Host.ToLowerInvariant();
            return m_AllowedHostSuffixes.Any(p_S => v_Host == p_S || v_Host.EndsWith("." + p_S, StringComparison.Ordinal));
        }
    }
}
