using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StorageSystem.EndToEndTests.Common;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string TestUserEmail = "e2e@storage.local";
    public const string TestUserName = "E2E User";
    public const string TestUserSubject = "e2e-subject";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserSubject),
            new Claim("sub", TestUserSubject),
            new Claim(ClaimTypes.Name, TestUserName),
            new Claim("name", TestUserName),
            new Claim(ClaimTypes.Email, TestUserEmail),
            new Claim("email", TestUserEmail)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
