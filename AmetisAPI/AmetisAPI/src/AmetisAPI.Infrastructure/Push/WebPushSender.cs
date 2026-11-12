using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using AmetisAPI.Domain.Bo.Push;
using AmetisAPI.Domain.Interfaces.Push;
using AmetisAPI.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AmetisAPI.Infrastructure.Push
{
    /// <summary>Envoi Web Push (RFC 8030) chiffré (RFC 8291) et authentifié par VAPID (RFC 8292). Les adresses d'abonnement ne sont pas journalisées.</summary>
    internal class WebPushSender(HttpClient p_Http, IOptions<PushOptions> p_Options, ILogger<WebPushSender> p_Logger) : IPushSender
    {
        private const int TimeToLiveSeconds = 24 * 3600;
        private readonly PushOptions m_Options = p_Options.Value;

        public bool IsEnabled => !string.IsNullOrWhiteSpace(m_Options.PublicKey) && !string.IsNullOrWhiteSpace(m_Options.PrivateKey) && !string.IsNullOrWhiteSpace(m_Options.Subject);

        public string PublicKey => m_Options.PublicKey;

        public async Task<PushResult> SendAsync(PushSubscriptionInfo p_Subscription, PushMessage p_Message, CancellationToken p_CancellationToken)
        {
            try
            {
                Uri v_Endpoint = new(p_Subscription.Endpoint);
                byte[] v_Body = WebPushEncryptor.Encrypt(JsonSerializer.SerializeToUtf8Bytes(new
                {
                    title = p_Message.Title,
                    body = p_Message.Body.Length > 300 ? p_Message.Body[..299] + "…" : p_Message.Body,
                    url = p_Message.Url,
                    tag = p_Message.Tag
                }), Base64Url.Decode(p_Subscription.P256dh), Base64Url.Decode(p_Subscription.Auth));

                using ECDsa v_Key = VapidKeys.Load(m_Options.PublicKey, m_Options.PrivateKey);
                string v_Token = VapidKeys.CreateToken(v_Key, v_Endpoint.GetLeftPart(UriPartial.Authority), m_Options.Subject, DateTime.UtcNow.AddHours(12));

                using HttpRequestMessage v_Request = new(HttpMethod.Post, v_Endpoint) { Content = new ByteArrayContent(v_Body) };
                v_Request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                v_Request.Content.Headers.ContentEncoding.Add("aes128gcm");
                v_Request.Headers.TryAddWithoutValidation("TTL", TimeToLiveSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                v_Request.Headers.TryAddWithoutValidation("Urgency", "normal");
                v_Request.Headers.TryAddWithoutValidation("Authorization", $"vapid t={v_Token}, k={m_Options.PublicKey}");

                using HttpResponseMessage v_Response = await p_Http.SendAsync(v_Request, p_CancellationToken);
                if (v_Response.IsSuccessStatusCode)
                    return PushResult.Sent;
                if (v_Response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
                    return PushResult.Gone;
                p_Logger.LogWarning("Web Push refusé par {Host} : HTTP {Status}", v_Endpoint.Host, (int)v_Response.StatusCode);
                return PushResult.Failed;
            }
            catch (Exception v_Ex) when (v_Ex is HttpRequestException or TaskCanceledException or FormatException or ArgumentException or CryptographicException)
            {
                p_Logger.LogWarning("Web Push en échec : {Error}", v_Ex.GetType().Name);
                return PushResult.Failed;
            }
        }
    }
}
