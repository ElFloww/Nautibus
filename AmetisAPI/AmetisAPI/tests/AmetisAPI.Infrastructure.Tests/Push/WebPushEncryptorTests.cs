using System.Security.Cryptography;
using AmetisAPI.Infrastructure.Push;

namespace AmetisAPI.Infrastructure.Tests.Push
{
    /// <summary>Valeurs de l'annexe A de la RFC 8291 (https://www.rfc-editor.org/rfc/rfc8291.txt).</summary>
    public class WebPushEncryptorTests
    {
        private const string Plaintext = "V2hlbiBJIGdyb3cgdXAsIEkgd2FudCB0byBiZSBhIHdhdGVybWVsb24";
        private const string AsPublic = "BP4z9KsN6nGRTbVYI_c7VJSPQTBtkgcy27mlmlMoZIIgDll6e3vCYLocInmYWAmS6TlzAC8wEqKK6PBru3jl7A8";
        private const string AsPrivate = "yfWPiYE-n46HLnH0KqZOF1fJJU3MYrct3AELtAQ-oRw";
        private const string UaPublic = "BCVxsr7N_eNgVRqvHtD0zTZsEc6-VV-JvLexhqUzORcxaOzi6-AYWXvTBHm4bjyPjs7Vd8pZGH6SRpkNtoIAiw4";
        private const string Salt = "DGv6ra1nlYgDCS1FRnbzlw";
        private const string AuthSecret = "BTBZMqHH6r4Tts7J_aSIgg";
        private const string ExpectedHeader = "DGv6ra1nlYgDCS1FRnbzlwAAEABBBP4z9KsN6nGRTbVYI_c7VJSPQTBtkgcy27mlmlMoZIIgDll6e3vCYLocInmYWAmS6TlzAC8wEqKK6PBru3jl7A8";
        private const string ExpectedCiphertext = "8pfeW0KbunFT06SuDKoJH9Ql87S1QUrdirN6GcG7sFz1y1sqLgVi1VhjVkHsUoEsbI_0LpXMuGvnzQ";

        [Fact]
        public void Encryption_Matches_Rfc8291_Appendix_A()
        {
            byte[] v_AsPublic = Base64Url.Decode(AsPublic);
            using ECDiffieHellman v_As = ECDiffieHellman.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                D = Base64Url.Decode(AsPrivate),
                Q = new ECPoint { X = v_AsPublic[1..33], Y = v_AsPublic[33..65] }
            });

            byte[] v_Body = WebPushEncryptor.Encrypt(Base64Url.Decode(Plaintext), Base64Url.Decode(UaPublic), Base64Url.Decode(AuthSecret), v_As, Base64Url.Decode(Salt));

            byte[] v_Header = Base64Url.Decode(ExpectedHeader);
            Check.That(v_Body.Length).IsEqualTo(144);
            Check.That(Base64Url.Encode(v_Body[..v_Header.Length])).IsEqualTo(ExpectedHeader);
            Check.That(Base64Url.Encode(v_Body[v_Header.Length..])).IsEqualTo(ExpectedCiphertext);
        }

        [Fact]
        public void Oversized_Payload_Is_Refused()
        {
            Check.ThatCode(() => WebPushEncryptor.Encrypt(new byte[WebPushEncryptor.MaxPayloadLength + 1], Base64Url.Decode(UaPublic), new byte[16])).Throws<ArgumentException>();
        }
    }
}
