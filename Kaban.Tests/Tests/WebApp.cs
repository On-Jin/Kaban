using Kaban.Tests.Setup;
using Snapshooter.Xunit;

namespace Kaban.Tests.Tests;

[Collection(nameof(WebAppCollectionFixture))]
public class WebApp : IAsyncLifetime
{
    private readonly HttpClient _httpClient;

    private readonly Func<Task> _resetDatabase;

    public WebApp(WebAppFactory factory)
    {
        _resetDatabase = factory.ResetDatabaseAsync;

        _httpClient = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _resetDatabase();
    
    [Fact]
    public async Task App_Can_Run_Query()
    {
        StringContent httpContent = new("{ \"query\":\"{ authors { name books { title } } }\" }",
            System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/graphql", httpContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        responseContent.MatchSnapshot();
    }
}