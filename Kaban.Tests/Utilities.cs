using System.Text.RegularExpressions;
using Kaban.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kaban.Tests;

static class Utilities
{
    public static async Task<string> FileGqlToJsonQuery(string filePath, string input = "", string? mutationName = null)
    {
        string graphqlQuery = await File.ReadAllTextAsync(filePath);
        input = Regex.Replace(input, "[{,](.+?:)", m => m.Groups[0].Value.Replace("\"", String.Empty));
        if (mutationName != null)
            graphqlQuery = graphqlQuery.Replace("TEST_MUTATION_NAME", mutationName);
        return $"{{ \"query\": \"{graphqlQuery.Replace("TEST_INPUT", input).Replace(System.Environment.NewLine, "\\n").Replace("\"", "\\\"")}\" }}";
    }
}

// public class WrapperServiceScope : IDisposable
// {
//     private readonly IServiceScope _scope;
//     private AppDbContext? _dbContext;
//
//     public WrapperServiceScope(WebApplicationFactory<global::Program> app)
//     {
//         _scope = app.Services.CreateScope();
//     }
//
//     public async Task<AppDbContext> GetDbContext()
//     {
//         IServiceProvider scopedServices = _scope.ServiceProvider;
//         IDbContextFactory<AppDbContext> db = scopedServices.GetRequiredService<IDbContextFactory<AppDbContext>>();
//         _dbContext = await db.CreateDbContextAsync();
//         return _dbContext;
//     }
//
//     void IDisposable.Dispose()
//     {
//         _scope.Dispose();
//         _dbContext?.Dispose();
//     }
// }