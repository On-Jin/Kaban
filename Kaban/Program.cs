using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Kaban;
using Kaban.Data;
using Kaban.Models;
using Kaban.Mutations;
using Kaban.Query;
using Kaban.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie", o =>
    {
        o.LoginPath = "/login";
        o.Cookie.Name = "default";
    })
    .AddOAuth("discord", o =>
    {
        o.SignInScheme = "cookie";
        o.ClientId = SecretHelper.GetSecret(builder, "DISCORD_CLIENT_ID")!;
        o.ClientSecret = SecretHelper.GetSecret(builder, "DISCORD_SECRET")!;
        o.AuthorizationEndpoint = "https://discord.com/oauth2/authorize";
        o.TokenEndpoint = "https://discord.com/api/oauth2/token";
        o.CallbackPath = "/oauth/discord-cb";
        o.UserInformationEndpoint = "https://discord.com/api/users/@me";
        o.SaveTokens = true;
        o.Scope.Add("identify");

        o.ClaimActions.MapJsonKey("urn:discord:id", "id");
        o.ClaimActions.MapJsonKey("urn:discord:username", "username");
        o.ClaimActions.MapJsonKey("urn:discord:discriminator", "discriminator");
        o.ClaimActions.MapJsonKey("urn:discord:avatar", "avatar");

        // https://discord.com/oauth2/authorize?client_id=1237789341393096779&response_type=code&redirect_uri=http%3A%2F%2Flocalhost%3A5264%2Foauth%2Fdiscord-cb&scope=identify
        o.Events.OnTicketReceived = ctx => { return Task.CompletedTask; };

        o.Events.OnCreatingTicket = async ctx =>
        {
            var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var userService = ctx.HttpContext.RequestServices.GetRequiredService<IUserService>();


            var handlers = ctx.HttpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            var handler = await handlers.GetHandlerAsync(ctx.HttpContext, "cookie");
            var authResult = await handler!.AuthenticateAsync();

            User? user;
            if (!authResult.Succeeded)
            {
                user = await userService.Create();
            }
            else
            {
                // Get Name from Cookie token claims
                var userId = authResult.Principal!.Identity!.Name!;
                Console.WriteLine(userId);
                var id = new Guid(userId!);
                user = db.Users.Find(id)!;
            }

            ctx.Identity!.AddClaim(new Claim(ClaimTypes.Name, user.Id.ToString()));


            using var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);
            using var result = await ctx.Backchannel.SendAsync(request);

            var discordUser = await result.Content.ReadFromJsonAsync<JsonElement>();
            ctx.RunClaimActions(discordUser);

            var discordId = ctx.Principal!.Claims.First(c => c.Type == "urn:discord:id");
            user.DiscordId = discordId.Value;
            db.SaveChanges();
        };
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("visitor", pb => { pb.RequireAuthenticatedUser().AddAuthenticationSchemes("cookie"); })
    .AddPolicy("discord-enabled", pb =>
    {
        pb.RequireAuthenticatedUser()
            .AddAuthenticationSchemes("discord")
            .RequireClaim("urn:discord:id");
    });

builder.Services.AddDbContextPool<AppDbContext>(optionsBuilder =>
{
    var cs = SecretHelper.GetSecret(builder, "KABANDBCONNECTIONSTRING") ??
             builder.Configuration.GetConnectionString("KABANDBCONNECTIONSTRING");
    optionsBuilder.UseNpgsql(cs);
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<CustomAuthenticationMiddleware>();

builder.Services
    .AddGraphQLServer()
    .RegisterDbContext<AppDbContext>()
    .AddQueryType<Query>()
    .AddProjections()
    .AddAuthorization()
    // .AddMutationType<Mutation>()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = isDevelopment);


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}


app.UseAuthentication();
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseMiddleware<CustomAuthenticationMiddleware>();


app.MapGraphQL();

app.MapGet("/", () => "Hello!");


app.MapGet("/protected", async (ctx) =>
{
    string s = "";
    foreach (var userClaim in ctx.User.Claims)
    {
        s += $"    {userClaim.Type} : {userClaim.Value}\n";
    }

    foreach (var claimsIdentity in ctx.User.Identities)
    {
        s += $"Identity : [{claimsIdentity.Name}] [{claimsIdentity.Label}] [{claimsIdentity.AuthenticationType}]\n";
        s += $"  Claims :\n";
        foreach (var claimsIdentityClaim in claimsIdentity.Claims)
        {
            s += $"    {claimsIdentityClaim.Type} : {claimsIdentityClaim.Value}\n";
        }
    }

    await ctx.Response.WriteAsync(s);
}).RequireAuthorization("visitor");

// app.MapGet("/login", async (HttpContext ctx) =>
// {
//     var db = ctx.RequestServices.GetRequiredService<AppDbContext>();
//
//     User user = new User();
//     db.Users.Add(user);
//     db.SaveChanges();
//
//     var ci = new ClaimsPrincipal(new[]
//     {
//         new ClaimsIdentity(new List<Claim>()
//             {
//                 new Claim(ClaimTypes.Name, user.Id.ToString())
//             },
//             "cookie")
//     });
//     var authProperties = new AuthenticationProperties
//     {
//         IsPersistent = true,
//     };
//     await ctx.SignInAsync("cookie",
//         new ClaimsPrincipal(ci),
//         authProperties);
//     return "ok";
// });

app.MapGet("/didi", async (IUserService userService, AppDbContext db, HttpContext ctx) =>
    {
        Console.WriteLine("didi");
        var userId = ctx.User.Identity!.Name!;
        var user = (await userService.Find(userId))!;

        Console.WriteLine($"{user.Id} {user.DiscordId}");
        // await ctx.Response.WriteAsync($"{user.Id} {user.DiscordId}");
        ctx.Response.Redirect("http://localhost:3000");
    }).RequireAuthorization("discord-enabled")
    ;

// app.MapGet("/discord-login", async ctx => { await ctx.ChallengeAsync("discord"); });

app.MapGet("/logout", async ctx => { await ctx.SignOutAsync(); });


app.Run();

// https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0#sut-environment
namespace Kaban
{
    public partial class Program;
}