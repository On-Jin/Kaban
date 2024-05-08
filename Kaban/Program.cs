using System.Security.Claims;
using Kaban;
using Kaban.Data;
using Kaban.Models;
using Kaban.Query;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

builder.Services.AddDbContextPool<AppDbContext>(optionsBuilder =>
{
    var cs = SecretHelper.GetSecret("KABANDBCONNECTIONSTRING") ??
             builder.Configuration.GetConnectionString("KABANDBCONNECTIONSTRING");
    optionsBuilder.UseNpgsql(cs);
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddGraphQLServer()
    .RegisterDbContext<AppDbContext>()
    .AddQueryType<Query>()
    .AddProjections()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = isDevelopment);


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapGraphQL();

app.MapGet("/", () => "Hello!");

app.MapGet("/protected", (ctx) =>
{
    string s = "";
    if (ctx.User.Identity != null)
        s += ctx.User.Identity.Name;
    s += $"\n{ctx.User.Claims.First().Value}";
    return ctx.Response.WriteAsync(s);
}).RequireAuthorization();

app.MapGet("/login", (HttpContext ctx) =>
{
    var db = ctx.RequestServices.GetRequiredService<AppDbContext>();

    User user = new User();
    db.Users.Add(user);
    db.SaveChanges();
    Console.WriteLine(user);

    ctx.SignInAsync(new ClaimsPrincipal(new[]
    {
        new ClaimsIdentity(new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Id.ToString())
            },
            CookieAuthenticationDefaults.AuthenticationScheme)
    }));
    return "ok";
});

app.Run();

// https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0#sut-environment
namespace Kaban
{
    public partial class Program;
}