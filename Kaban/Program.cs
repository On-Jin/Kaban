using Kaban;
using Kaban.Data;
using Kaban.Query;
using Microsoft.EntityFrameworkCore;

bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<AppDbContext>(optionsBuilder =>
{
    var cs = SecretHelper.GetSecret("KABANDBCONNECTIONSTRING");
    if (cs == null)
    {
        cs = builder.Configuration.GetConnectionString("KABANDBCONNECTIONSTRING");
    }

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

app.UseHttpsRedirection();

app.MapGraphQL();

app.MapGet("/", () => "Hello!");

app.Run();

// https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0#sut-environment
public partial class Program;