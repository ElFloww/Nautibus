using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AmetisAPI.Infrastructure.Push
{
    /// <summary>Clés et jeton VAPID (RFC 8292) : JWT ES256 signé avec la clé privée du serveur.</summary>
    public static class VapidKeys
    {
        /// <summary>Nouvelle paire de clés (publique, privée) en base64url.</summary>
        public static (string PublicKey, string PrivateKey) Generate()
        {
            using ECDsa v_Key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            ECParameters v_Parameters = v_Key.ExportParameters(true);
            return (Base64Url.Encode([0x04, .. v_Parameters.Q.X, .. v_Parameters.Q.Y]), Base64Url.Encode(v_Parameters.D));
        }

        internal static ECDsa Load(string p_PublicKey, string p_PrivateKey)
        {
            byte[] v_Public = Base64Url.Decode(p_PublicKey);
            return ECDsa.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint { X = v_Public[1..33], Y = v_Public[33..65] },
                D = Base64Url.Decode(p_PrivateKey)
            });
        }

        /// <summary>Jeton pour l'origine du service push (« aud »), valable jusqu'à <paramref name="p_ExpiresUtc"/> (24 h maximum).</summary>
        internal static string CreateToken(ECDsa p_Key, string p_Audience, string p_Subject, DateTime p_ExpiresUtc)
        {
            string v_Header = Base64Url.Encode(Encoding.UTF8.GetBytes("{\"typ\":\"JWT\",\"alg\":\"ES256\"}"));
            string v_Claims = Base64Url.Encode(JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, object>
            {
                ["aud"] = p_Audience,
                ["exp"] = new DateTimeOffset(p_ExpiresUtc).ToUnixTimeSeconds(),
                ["sub"] = p_Subject
            }));
            // Signature au format IEEE P1363 (r || s), celui attendu par JWS ES256.
            byte[] v_Signature = p_Key.SignData(Encoding.ASCII.GetBytes($"{v_Header}.{v_Claims}"), HashAlgorithmName.SHA256);
            return $"{v_Header}.{v_Claims}.{Base64Url.Encode(v_Signature)}";
        }
    }
}
