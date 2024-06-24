namespace OAuthCodePkceFlow
{
    public class Pkce
    {
        public Pkce(string codeVerifier, string codeChallenge, string codeChallengeMethod)
        {
            CodeVerifier = codeVerifier;
            CodeChallenge = codeChallenge;
            CodeChallengeMethod = codeChallengeMethod;
        }

        public string CodeVerifier { get; private set; }
        public string CodeChallenge { get; private set; }
        public string CodeChallengeMethod { get; private set; }
    }
}
