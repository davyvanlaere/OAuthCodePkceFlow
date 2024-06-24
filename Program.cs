using IdentityModel.Client;
using OAuthCodePkceFlow;
using System.Diagnostics;
using System.Net;

var redirectionUri = "https://localhost:7181/acceptcode";
var pkce = PkceGenerator.Generate(128); 
var settings = StartRedirectionHost();

// Create authorize uri with code_challenge
// PKCE flow: The code_challenge is a SHA256 hash of a secret that we generated ourself and is only known to us.
// The identity server returns an authorization code, and will remember which hash was passed for this authorization code.
// When we do the second request to exchange the authorization code for an access token, we will need to pass the actual secret.
// The identity server will then apply a SHA256 hash to the secret, and verify if that hash matches the hash that we passed in our first request.
// An malicious party might intercept the authorization code, but if they dont have the generated secret, the authorization code is useless.
var authorizeUrl = new RequestUrl(settings.AuthenticationUri).CreateAuthorizeUrl(
                responseType: "code",
                scope: settings.Scope,
                clientId: settings.ClientId,
                redirectUri: redirectionUri,
                codeChallenge: pkce.CodeChallenge,
                codeChallengeMethod: pkce.CodeChallengeMethod);

Console.WriteLine($"Opening browser window for uri {authorizeUrl}");

// open logon screen
Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = authorizeUrl });

Console.WriteLine("Waiting for incoming redirect call, press any key to cancel.");
Console.Read();

Settings StartRedirectionHost()
{
    var builder = WebApplication.CreateBuilder(args);
    settings = builder.Configuration.GetSection("Settings").Get<Settings>();
    var app = builder.Build();

    app.MapGet("/acceptcode", async (string code) =>
    {
        Console.WriteLine($"Received authorization code {code}");
        var token = await GetToken(code, pkce.CodeVerifier, redirectionUri);
        return new HtmlResult($@"
        <html>
            <head>
                <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/bootstrap@4.4.1/dist/css/bootstrap.min.css"">
            </head>
            <body>
                <table class=""table table-striped"">
                    <tr>
                        <td>Access Token</td>
                        <td style=""width:600px; word-wrap:break-word; display:inline-block;"">{token.AccessToken}</td>
                    </tr>
                    <tr>
                        <td>Refresh Token</td>
                        <td style=""width:600px; word-wrap:break-word; display:inline-block;"">{token.RefreshToken}</td>
                    </tr>
                    <tr>
                        <td>Scope</td>
                        <td>{token.Scope}</td>
                    </tr>
                    <tr>
                        <td>ExpiresIn</td>
                        <td>{token.ExpiresIn}</td>
                    </tr>
                </table>
            </body>
        </html>");
    });

    Task.Run(() => app.Run());

    return settings;
}

async Task<TokenResponse> GetToken(string authorizationCode, string pkceVerifier, string redirectUri)
{
    var request = new AuthorizationCodeTokenRequest()
    {
        Address = settings.TokenUri,
        Code = authorizationCode,
        GrantType = "authorization_code",
        CodeVerifier = pkceVerifier,
        ClientId = settings.ClientId,
        RedirectUri = redirectUri
    };

    using (var httpClient = new HttpClient())
    {
        var response = await httpClient.RequestAuthorizationCodeTokenAsync(request);
        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine($"AccessToken: {response.AccessToken}");
            Console.WriteLine(string.Empty);
            Console.WriteLine($"RefreshToken: {response.RefreshToken}");
            Console.WriteLine(string.Empty);
            Console.WriteLine($"Scope: {response.Scope}");
            Console.WriteLine(string.Empty);
            Console.WriteLine($"ExpiresIn: {response.ExpiresIn}");
        }
        else
        {
            Console.WriteLine($"Failed to exhange the authorization code for a token: request failed with status {response.HttpStatusCode}, error: {response.Error}");
        }
        return response;
    }
}
