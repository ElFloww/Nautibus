using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AmetisAPI.Infrastructure.Push;

namespace AmetisAPI.Infrastructure.Tests.Push
{
    public class VapidKeysTests
    {
        [Fact]
        public void Token_Is_An_Es256_Jwt_Signed_With_The_Generated_Key()
        {
            (string v_Public, string v_Private) = VapidKeys.Generate();
            Check.That(Base64Url.Decode(v_Public)).HasSize(65);
            Check.That(Base64Url.Decode(v_Private)).HasSize(32);

            using ECDsa v_Key = VapidKeys.Load(v_Public, v_Private);
            string v_Token = VapidKeys.CreateToken(v_Key, "https://fcm.googleapis.com", "mailto:contact@exemple.test", new DateTime(2026, 11, 12, 22, 0, 0, DateTimeKind.Utc));
            string[] v_Parts = v_Token.Split('.');

            using JsonDocument v_Claims = JsonDocument.Parse(Base64Url.Decode(v_Parts[1]));
            Check.That(v_Claims.RootElement.GetProperty("aud").GetString()).IsEqualTo("https://fcm.googleapis.com");
            Check.That(v_Claims.RootElement.GetProperty("exp").GetInt64()).IsEqualTo(1794520800);
            Check.That(Encoding.UTF8.GetString(Base64Url.Decode(v_Parts[0]))).Contains("\"alg\":\"ES256\"");

            byte[] v_PublicKey = Base64Url.Decode(v_Public);
            using ECDsa v_Verifier = ECDsa.Create(new ECParameters { Curve = ECCurve.NamedCurves.nistP256, Q = new ECPoint { X = v_PublicKey[1..33], Y = v_PublicKey[33..65] } });
            Check.That(v_Verifier.VerifyData(Encoding.ASCII.GetBytes($"{v_Parts[0]}.{v_Parts[1]}"), Base64Url.Decode(v_Parts[2]), HashAlgorithmName.SHA256)).IsTrue();
        }
    }
}
