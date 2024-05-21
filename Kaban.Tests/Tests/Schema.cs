using HotChocolate.Execution;
using Kaban.Tests.Setup;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.Xunit;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests;

[Collection(nameof(WebAppCollectionFixture))]
public class Schema : IAsyncLifetime
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IRequestExecutor _executor;

    private readonly Func<Task> _resetDatabase;

    public Schema(WebAppFactory factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _resetDatabase = factory.ResetDatabaseAsync;
        var serviceScope = factory.Services.CreateScope();
        _executor = serviceScope.ServiceProvider.GetRequiredService<IRequestExecutorResolver>()
            .GetRequestExecutorAsync().Result;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _resetDatabase();

    [Fact]
    public void SchemaChangeTest()
    {
        _testOutputHelper.WriteLine(_executor.Schema.Print());
        _executor.Schema.Print().MatchSnapshot();
    }
}