using System.Security.Claims;
using Kaban.Data;
using Kaban.Models;
using Microsoft.AspNetCore.Authentication;

namespace Kaban;

public class CustomAuthenticationMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        Console.WriteLine($"CustomAuthenticationMiddleware {context.Request.Path}");
        
        foreach (var claimsIdentity in context.User.Identities)
        {
            Console.WriteLine($"Identity : {claimsIdentity.Name}");
        }
        if (context.User.Identity.IsAuthenticated)
        {
            // Access the claims
            var claims = context.User.Claims;
                
            // Perform actions based on claims
            foreach (var claim in claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
                // Perform your logic here based on the claims
            }
        }
        else
        {
            var db = context.RequestServices.GetRequiredService<AppDbContext>();

            User user = new User();
            db.Users.Add(user);
            db.SaveChanges();

            var ci = new ClaimsPrincipal(new[]
            {
                new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString())
                    },
                    "cookie")
            });
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
            };
            await context.SignInAsync("cookie",
                new ClaimsPrincipal(ci),
                authProperties);
        }
        await next.Invoke(context);
    }
}