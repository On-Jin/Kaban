using HotChocolate;
using HotChocolate.Execution;
using Kaban.Tests.Setup;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.Xunit;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests;

[Collection(nameof(WebAppCollectionFixture))]
public class GraphQL : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;

    private readonly IRequestExecutor _executor;

    private readonly Func<Task> _resetDatabase;

    public GraphQL(WebAppFactory factory, ITestOutputHelper output)
    {
        _output = output;
        _resetDatabase = factory.ResetDatabaseAsync;

        var serviceScope = factory.Services.CreateScope();
        _executor = serviceScope.ServiceProvider.GetRequiredService<IRequestExecutorResolver>()
            .GetRequestExecutorAsync().Result;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _resetDatabase();

    [Fact]
    public async Task QueryAllAuthor()
    {
        var result = await _executor.ExecuteAsync("{ authors { name books { title } } }");

        result.ToJson().MatchSnapshot();
    }
}