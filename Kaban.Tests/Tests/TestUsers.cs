using System.Security.Claims;

namespace Kaban.Tests.Tests;

public static class TestUsers
{
    public static ClaimsPrincipal GetTestUser(string username)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, username),
            // Add other desired claims
        };

        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }
}