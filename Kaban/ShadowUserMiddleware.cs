using System.Runtime.InteropServices.ComTypes;
using System.Security.Claims;
using Kaban.Data;
using Kaban.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;

namespace Kaban;

public class ShadowUserMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments("/graphql"))
        {
        }
        else if (context.User.Identity is { IsAuthenticated: false })
        {
            if (!context.Session.TryGetValue("ShadowUserId", out var shadowUserId))
            {
                var user = new User();
                var db = context.RequestServices.GetRequiredService<AppDbContext>();
                db.Users.Add(user);
                await db.SaveChangesAsync();

                shadowUserId = user.Id.ToByteArray();
                context.Session.Set("ShadowUserId", shadowUserId);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, new Guid(shadowUserId).ToString()),
                // new Claim(ClaimTypes.Name, "ShadowUser")
            };

            var identity = new ClaimsIdentity(claims, "Shadow");
            context.User = new ClaimsPrincipal(identity);
            Console.WriteLine($"2 {context.Request.GetDisplayUrl()}");
            Console.WriteLine(new Guid(shadowUserId).ToString());
            await context.SignInAsync(new ClaimsPrincipal(identity));
        }

        await next.Invoke(context);
    }
}