using System.Security.Cryptography;
using System.Text;

namespace OAuthCodePkceFlow
{
    public static class PkceGenerator
    {
        public static Pkce Generate(int length)
        {
            // https://www.rfc-editor.org/rfc/rfc7636#section-4.1

            var rfc3986Bytes = GetRFC3986Bytes();

            var randomBytes = new byte[length];
            RandomNumberGenerator.Create().GetBytes(randomBytes);
            var codeVerifierBytes = randomBytes.Select(rnd => rfc3986Bytes[rnd % rfc3986Bytes.Count]).ToArray();
            var codeVerifier = Encoding.UTF8.GetString(codeVerifierBytes);

            string codeChallenge = null;
            using (var sha256 = SHA256.Create())
            {
                var codeChallengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                codeChallenge = ToBase64UrlEncoded(codeChallengeBytes);
            }

            return new Pkce(codeVerifier, codeChallenge, "S256");
        }

        public static string ToBase64UrlEncoded(byte[] data)
        {
            // https://www.rfc-editor.org/rfc/rfc7636#appendix-A
            return Convert.ToBase64String(data).Replace("+", "-").Replace("/", "_").Replace("=", String.Empty);
        }

        private static List<byte> GetRFC3986Bytes()
        {
            var rfc3986Bytes = Enumerable.Range('0', 10)
                .Union(Enumerable.Range('a', 26))
                .Union(Enumerable.Range('A', 26)).Select(i => (byte)i).ToList();

            rfc3986Bytes.Add((byte)'-');
            rfc3986Bytes.Add((byte)'_');
            rfc3986Bytes.Add((byte)'.');
            rfc3986Bytes.Add((byte)'~');

            return rfc3986Bytes;
        }
    }
}
