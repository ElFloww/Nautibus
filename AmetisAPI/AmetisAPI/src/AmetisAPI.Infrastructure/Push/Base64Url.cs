namespace AmetisAPI.Infrastructure.Push
{
    internal static class Base64Url
    {
        public static string Encode(ReadOnlySpan<byte> p_Bytes) => Convert.ToBase64String(p_Bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        public static byte[] Decode(string p_Text)
        {
            string v_Text = p_Text.Trim().Replace('-', '+').Replace('_', '/');
            return Convert.FromBase64String(v_Text.PadRight(v_Text.Length + (4 - v_Text.Length % 4) % 4, '='));
        }
    }
}
