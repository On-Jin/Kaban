using System.Text.Json;
using System.Text.Json.Nodes;
using Kaban.Data;
using Kaban.GraphQL.Boards;
using Kaban.Tests.Setup;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter;
using Snapshooter.Xunit;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests.TestBoard;

[Collection(nameof(WebAppCollectionFixture))]
public class TestBoard(WebAppFactory factory, ITestOutputHelper testOutputHelper)
    : TestBase(factory, testOutputHelper)
{
    [Fact]
    public async Task GraphQL_QueryBoard()
    {
        await ResetAndPopulateDb();

        (await GetQueryBoardsString()).MatchSnapshot();
    }

    [Fact]
    public async Task GraphQL_AddBoard()
    {
        await ResetAndPopulateDb();

        HttpResponseMessage response = await AddBoard(HttpClientShadow);

        string jsonWrapped = await response.Content.ReadAsStringAsync();
        TestOutputHelper.WriteLine(jsonWrapped);

        jsonWrapped.MatchSnapshot();

        (await GetQueryBoardsString()).MatchSnapshot(SnapshotNameExtension.Create("FullBoardQuery"));
    }

    [Fact]
    public async Task GraphQL_DeleteBoard()
    {
        await ResetAndPopulateDb();



        var response = await AddBoard(HttpClientShadow);
        var jsonWrapped = await response.Content.ReadAsStringAsync();
        TestOutputHelper.WriteLine(jsonWrapped);
        var jsonObject = JsonNode.Parse(jsonWrapped)!.AsObject();
        var id = int.Parse(jsonObject["data"]!["addBoard"]!["board"]!["id"]!.ToString());
        TestOutputHelper.WriteLine($"{id}");

        await DeleteBoard(HttpClientShadow, new DeleteBoardInput(id));
        (await GetQueryBoardsString()).MatchSnapshot();
    }


    [Fact]
    public async Task GraphQL_PatchBoard()
    {
        await ResetDatabase();
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await GenerateKabanBoardToShadowUser(db);

        var response = await PatchBoard(HttpClientShadow);
        JsonSerializer
            .Serialize(
                JsonSerializer.Deserialize<object>(await response.Content.ReadAsStringAsync()),
                TestHelper.JsonSerializerOptions)
            .MatchSnapshot();
    }
}