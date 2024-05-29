using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Kaban;
using Kaban.Data;
using Kaban.GraphQL.Boards;
using Kaban.GraphQL.MainTasks;
using Kaban.Models;
using Kaban.Mutations;
using Kaban.Query;
using Kaban.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "discord";
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o => { o.Cookie.HttpOnly = false; })
    .AddOAuth("discord", o =>
    {
        o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
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
            Console.WriteLine("OnCreatingTicket");
            var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await ctx.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                ctx.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            ctx.RunClaimActions(user.RootElement);
        };
    });

builder.Services.AddDbContextPool<AppDbContext>(optionsBuilder =>
{
    var cs = SecretHelper.GetSecret(builder, "KABANDBCONNECTIONSTRING") ??
             builder.Configuration.GetConnectionString("KABANDBCONNECTIONSTRING");
    optionsBuilder.UseNpgsql(cs);
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<ShadowUserMiddleware>();


builder.Services
    .AddGraphQLServer()
    .RegisterDbContext<AppDbContext>(DbContextKind.Pooled)
    .AddQueryType<Query>()
    .AddProjections()
    .AddAuthorization()
    .AddMutationType<Mutation>()
    .AddType<MainTaskType>()
    .AddType<BoardType>()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = isDevelopment);
builder.Services.AddSession();
builder.Services.AddMvc(o => { o.EnableEndpointRouting = false; });
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("shadow", pb => { pb.RequireAuthenticatedUser(); })
    .AddPolicy("discord-enabled", pb =>
    {
        pb.RequireAuthenticatedUser()
            .AddAuthenticationSchemes("discord")
            .RequireClaim("urn:discord:id");
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseMiddleware<ShadowUserMiddleware>();
app.UseMvcWithDefaultRoute();


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
});

app.MapGet("/me", async (IUserService userService, AppDbContext db, HttpContext ctx) =>
{
    Console.WriteLine("me");
    var userId = ctx.User.Identity!.Name!;
    var user = (await userService.Find(userId))!;
    await ctx.Response.WriteAsJsonAsync(new Me()
    {
        Id = user.Id!,
        DiscordUsername = user.DiscordUsername,
        DiscordAvatarUrl = user.DiscordId != null
            ? $"https://cdn.discordapp.com/avatars/{user.DiscordId}/{user.DiscordAvatar}"
            : null
    });
});

app.MapGet("/discord-login", async (HttpContext ctx, AppDbContext db, IUserService userService) =>
{
    var authenticateResult = await ctx.AuthenticateAsync("discord");

    if (!authenticateResult.Succeeded)
    {
        await ctx.ChallengeAsync("discord");
    }

    var claims = authenticateResult.Principal.Claims.ToList();

    // Get the ShadowUserId from the session
    if (ctx.Session.TryGetValue("ShadowUserId", out var shadowUserId))
    {
        var shadowUserIdString = new Guid(shadowUserId).ToString();
        Console.WriteLine("ShadowUser to NormalUser");

        var discordId = authenticateResult.Principal.Claims.First(c => c.Type == "urn:discord:id");
        var discordUser = await userService.FindByDiscordId(discordId.Value);

        // If discord User is already linked to a Shadow user, try to mix up Shadow data dans discord data
        if (discordUser != null)
        {
            var discordUsername = authenticateResult.Principal.Claims.First(c => c.Type == "urn:discord:username");
            var discordAvatar = authenticateResult.Principal.Claims.First(c => c.Type == "urn:discord:avatar");
            discordUser.DiscordId = discordId.Value;
            discordUser.DiscordUsername = discordUsername.Value;
            discordUser.DiscordAvatar = discordAvatar.Value;
            db.SaveChanges();

            claims.Add(new Claim(ClaimTypes.Name, discordUser.Id.ToString()));

            // delete user
            await userService.DeleteShadowUser(shadowUserIdString);
        }
        // Otherwise, add additional discord data to Shadow user
        else
        {
            var user = (await userService.Find(shadowUserIdString))!;
            var discordUsername = authenticateResult.Principal.Claims.First(c => c.Type == "urn:discord:username");
            var discordAvatar = authenticateResult.Principal.Claims.First(c => c.Type == "urn:discord:avatar");
            user.DiscordId = discordId.Value;
            user.DiscordUsername = discordUsername.Value;
            user.DiscordAvatar = discordAvatar.Value;
            db.SaveChanges();
            claims.Add(new Claim(ClaimTypes.Name, shadowUserIdString));
        }


        ctx.Session.Remove("ShadowUserId");
    }

    // Sign in the user with the merged claims
    await ctx.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
        new AuthenticationProperties { IsPersistent = true });

    ctx.Response.Redirect("/");
});

app.MapGet("/logout", async ctx => { await ctx.SignOutAsync(); });

app.Run();

// https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0#sut-environment
namespace Kaban
{
    public partial class Program;
}