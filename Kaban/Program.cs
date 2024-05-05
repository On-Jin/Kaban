using Kaban.Data;
using Kaban.Query;
using Microsoft.EntityFrameworkCore;

bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<AppDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("KabanDbConnectionString")!);
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

app.UseHttpsRedirection();

app.MapGraphQL();

app.Run();

// https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0#sut-environment
public partial class Program;