using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kaban.Tests;

public static class TestHelper
{
    public const string GuidDiscordAuth = "458ef677-f7a1-424a-bea4-0d6d8ec95717";
    public const string GuidShadowAuth = "158ef677-f7a1-424a-bea4-0d6d8ec95717";
    public const string HeaderUserGuid = "UserGuid";
}

public class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
    public string DefaultUserId { get; set; } = null!;
}

public class TestAuthHandler : AuthenticationHandler<TestAuthHandlerOptions>
{
    public const string AuthenticationScheme = "Test";
    private readonly string _defaultUserId;

    public TestAuthHandler(
        IOptionsMonitor<TestAuthHandlerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
        _defaultUserId = options.CurrentValue.DefaultUserId;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Context.Request.Headers.Authorization != AuthenticationScheme)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new Claim("urn:discord:id", "259484228503624441")
        };

        if (Context.Request.Headers.TryGetValue(TestHelper.HeaderUserGuid, out var userGuid))
        {
            claims.Add(new Claim(ClaimTypes.Name, userGuid[0]));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Name, TestHelper.GuidDiscordAuth));
        }

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}