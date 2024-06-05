using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Kaban.GraphQL.Columns;
using Kaban.Models.Dto;
using Kaban.Tests.Setup;
using Snapshooter;
using Snapshooter.Xunit;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests.TestColumn;

[Collection(nameof(WebAppCollectionFixture))]
public class TestColumn(WebAppFactory factory, ITestOutputHelper testOutputHelper)
    : TestColumnBase(factory, testOutputHelper)
{
    [Fact]
    public async Task GraphQL_AddColumn()
    {
        await ResetAndPopulateDb();

        {
            HttpResponseMessage response = await AddColumn(HttpClientShadow);

            string jsonWrapped = await response.Content.ReadAsStringAsync();
            TestOutputHelper.WriteLine(jsonWrapped);

            JsonSerializer
                .Serialize(JsonSerializer.Deserialize<object>(jsonWrapped), TestHelper.JsonSerializerOptions)
                .MatchSnapshot();
        }
        (await GetQueryBoardsString()).MatchSnapshot(SnapshotNameExtension.Create("FullBoardQuery"));
    }


    [Fact]
    public async Task GraphQL_DeleteColumn()
    {
        await ResetAndPopulateDb();

        int id = 0;
        {
            var response = await AddColumn(HttpClientShadow);
            var jsonWrapped = await response.Content.ReadAsStringAsync();
            TestOutputHelper.WriteLine(jsonWrapped);
            var jsonObject = JsonNode.Parse(jsonWrapped)!.AsObject();
            id = int.Parse(jsonObject["data"]!["addColumn"]!["column"]!["id"]!.ToString());
            TestOutputHelper.WriteLine($"{id}");
        }

        {
            await DeleteColumn(HttpClientShadow, new DeleteColumnInput(id));

            string jsonWrapped = await GetQueryBoardsString();
            var json = JsonNode.Parse(jsonWrapped)!["data"]!["boards"]!.ToString();
            var boardsDto = JsonSerializer.Deserialize<List<BoardDto>>(json,
                new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            boardsDto![0].Columns.Should().NotContain(c => c.Id == id);
        }
    }


    [Fact]
    public async Task GraphQL_PatchColumn()
    {
        await ResetAndPopulateDb();
        (await PatchColumn(HttpClientShadow)).MatchSnapshot();
    }
}