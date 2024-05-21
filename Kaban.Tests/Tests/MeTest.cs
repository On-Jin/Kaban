using System.Net.Http.Headers;
using Kaban.Tests.Setup;
using Snapshooter.Xunit;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests;

[Collection(nameof(WebAppCollectionFixture))]
public class MeTest : IAsyncLifetime
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _httpClientDiscord;
    private readonly HttpClient _httpClientShadow;

    private readonly Func<Task> _resetDatabase;

    public MeTest(WebAppFactory factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _resetDatabase = factory.ResetDatabaseAsync;

        _httpClientDiscord = factory.CreateClient();
        _httpClientDiscord.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        _httpClientShadow = factory.CreateClient();
        _httpClientShadow.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        _httpClientShadow.DefaultRequestHeaders.Add(TestHelper.HeaderUserGuid, TestHelper.GuidShadowAuth);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _resetDatabase();

    [Fact]
    public async Task Me_Shadow()
    {
        StringContent httpContent = new("{ \"query\":\"{ me { id discordUsername discordAvatarUrl } }\" }",
            System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClientShadow.PostAsync("/graphql", httpContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        _testOutputHelper.WriteLine(responseContent);
        responseContent.MatchSnapshot();
    }

    [Fact]
    public async Task Me_Discord()
    {
        StringContent httpContent = new("{ \"query\":\"{ me { id discordUsername discordAvatarUrl } }\" }",
            System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClientDiscord.PostAsync("/graphql", httpContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        _testOutputHelper.WriteLine(responseContent);
        responseContent.MatchSnapshot();
    }
}