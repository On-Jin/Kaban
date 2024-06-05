using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Kaban.Data;
using Kaban.Models;
using Kaban.Tests.Setup;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.Xunit;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests;

[Collection(nameof(WebAppCollectionFixture))]
public class Tests : IAsyncLifetime
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _httpClientDiscord;
    private readonly HttpClient _httpClientShadow;

    private readonly Func<Task> _resetDatabase;
    private readonly WebAppFactory _factory;

    public Tests(WebAppFactory factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
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

    private async Task GenerateKabanBoardToShadowUser(AppDbContext db)
    {
        var user = (await db.Users.FindAsync(new Guid(TestHelper.GuidShadowAuth)))!;
        var boards = await GenerateHelper.GenerateDefaultKabanBoards();
        user.Boards.AddRange(boards);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteOneBoardTestCascade()
    {
        await _resetDatabase();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await GenerateKabanBoardToShadowUser(db);

        {
            var user = (await db.Users.FindAsync(new Guid(TestHelper.GuidShadowAuth)))!;
            user.Boards.RemoveAt(0);
            await db.SaveChangesAsync();
        }
        {
            var user = (await db.Users.FindAsync(new Guid(TestHelper.GuidShadowAuth)))!;
            JsonSerializer.Serialize(user, TestHelper.JsonSerializerOptions).MatchSnapshot();
            Assert.Equal(2, db.Boards.Count());
        }
    }

    [Fact]
    public async Task AddGeneratedKabanBoardsTest()
    {
        await _resetDatabase();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await GenerateKabanBoardToShadowUser(db);

        {
            var user = (await db.Users.FindAsync(new Guid(TestHelper.GuidShadowAuth)))!;
            JsonSerializer.Serialize(user, TestHelper.JsonSerializerOptions).MatchSnapshot();
        }
    }

    [Fact]
    public async Task Test1()
    {
        await _resetDatabase();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        {
            var user = (await db.Users.FindAsync(new Guid(TestHelper.GuidShadowAuth)))!;
            var boards = await GenerateHelper.GenerateDefaultKabanBoards();
            user.Boards.AddRange(boards);
            await db.SaveChangesAsync();
        }

        {
            _testOutputHelper.WriteLine(db.Boards.ToList().Count.ToString());
            var user = (await db.Users.FindAsync(new Guid(TestHelper.GuidShadowAuth)))!;
            user.Boards.RemoveAt(0);
            await db.SaveChangesAsync();
            _testOutputHelper.WriteLine(db.Boards.ToList().Count.ToString());
        }

        {
            var user = (await db.Users.FindAsync(new Guid(TestHelper.GuidShadowAuth)))!;
            _testOutputHelper.WriteLine(JsonSerializer.Serialize(user, TestHelper.JsonSerializerOptions));
        }
        {
            _testOutputHelper.WriteLine(db.Users.ToList().Count.ToString());
            var user = (await db.Users.FindAsync(new Guid(TestHelper.GuidShadowAuth)))!;
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            _testOutputHelper.WriteLine(db.Users.ToList().Count.ToString());
        }
    }
}