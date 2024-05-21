using System.Net.Http.Headers;
using Kaban.Tests.Setup;
using Snapshooter.Xunit;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests;

[Collection(nameof(WebAppCollectionFixture))]
public class WebApp : IAsyncLifetime
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _httpClient;

    private readonly Func<Task> _resetDatabase;
    private readonly HttpClient _httpClientNonAuth;

    public WebApp(WebAppFactory factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _resetDatabase = factory.ResetDatabaseAsync;

        _httpClient = factory.CreateClient();
        _httpClientNonAuth = factory.CreateClient();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        // _httpClient.DefaultRequestHeaders.Add(TestAuthHandler.UserId, "1");
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

    [Fact]
    public async Task App_Cant_Run_Auth()
    {
        StringContent httpContent = new("{ \"query\":\"{ bookAuth { title } }\" }",
            System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClientNonAuth.PostAsync("/graphql", httpContent);
        var responseContent = await response.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine(responseContent);
        responseContent.MatchSnapshot();
    }

    [Fact]
    public async Task App_Can_Run_Auth()
    {
        StringContent httpContent = new("{ \"query\":\"{ bookAuth { title } }\" }",
            System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/graphql", httpContent);
        var responseContent = await response.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine(responseContent);
        responseContent.MatchSnapshot();
    }
}