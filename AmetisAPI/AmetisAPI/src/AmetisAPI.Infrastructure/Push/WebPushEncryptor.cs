using System.Buffers.Binary;
using System.Security.Cryptography;

namespace AmetisAPI.Infrastructure.Push
{
    /// <summary>Chiffrement d'un message Web Push (RFC 8291, codage aes128gcm de la RFC 8188, un seul enregistrement).</summary>
    internal static class WebPushEncryptor
    {
        public const int RecordSize = 4096;
        private const int TagLength = 16;
        private const int PublicKeyLength = 65;

        /// <summary>Taille maximale du message en clair pour tenir dans un enregistrement.</summary>
        public const int MaxPayloadLength = RecordSize - TagLength - 1;

        public static byte[] Encrypt(byte[] p_Payload, byte[] p_UserAgentPublicKey, byte[] p_AuthSecret)
        {
            using ECDiffieHellman v_Ephemeral = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
            return Encrypt(p_Payload, p_UserAgentPublicKey, p_AuthSecret, v_Ephemeral, RandomNumberGenerator.GetBytes(16));
        }

        /// <summary>Clé éphémère et sel fournis : permet de vérifier les valeurs de l'annexe A de la RFC 8291.</summary>
        internal static byte[] Encrypt(byte[] p_Payload, byte[] p_UserAgentPublicKey, byte[] p_AuthSecret, ECDiffieHellman p_ApplicationServerKey, byte[] p_Salt)
        {
            if (p_Payload.Length > MaxPayloadLength)
                throw new ArgumentException($"Message trop long pour un enregistrement ({p_Payload.Length} octets)", nameof(p_Payload));
            if (p_UserAgentPublicKey.Length != PublicKeyLength || p_UserAgentPublicKey[0] != 0x04)
                throw new ArgumentException("Clé p256dh invalide (point P-256 non compressé attendu)", nameof(p_UserAgentPublicKey));

            ECParameters v_As = p_ApplicationServerKey.ExportParameters(false);
            byte[] v_AsPublic = [0x04, .. v_As.Q.X, .. v_As.Q.Y];

            using ECDiffieHellman v_UserAgent = ECDiffieHellman.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint { X = p_UserAgentPublicKey[1..33], Y = p_UserAgentPublicKey[33..65] }
            });
            byte[] v_EcdhSecret = p_ApplicationServerKey.DeriveRawSecretAgreement(v_UserAgent.PublicKey);

            byte[] v_KeyInfo = [.. "WebPush: info\0"u8, .. p_UserAgentPublicKey, .. v_AsPublic];
            byte[] v_Ikm = HKDF.DeriveKey(HashAlgorithmName.SHA256, v_EcdhSecret, 32, p_AuthSecret, v_KeyInfo);
            byte[] v_Cek = HKDF.DeriveKey(HashAlgorithmName.SHA256, v_Ikm, 16, p_Salt, "Content-Encoding: aes128gcm\0"u8.ToArray());
            byte[] v_Nonce = HKDF.DeriveKey(HashAlgorithmName.SHA256, v_Ikm, 12, p_Salt, "Content-Encoding: nonce\0"u8.ToArray());

            byte[] v_Plain = [.. p_Payload, 0x02]; // délimiteur du dernier enregistrement
            byte[] v_Cipher = new byte[v_Plain.Length];
            byte[] v_Tag = new byte[TagLength];
            using (AesGcm v_Aes = new(v_Cek, TagLength))
                v_Aes.Encrypt(v_Nonce, v_Plain, v_Cipher, v_Tag);

            byte[] v_RecordSize = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(v_RecordSize, RecordSize);
            return [.. p_Salt, .. v_RecordSize, (byte)v_AsPublic.Length, .. v_AsPublic, .. v_Cipher, .. v_Tag];
        }
    }
}
