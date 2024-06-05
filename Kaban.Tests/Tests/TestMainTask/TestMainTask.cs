using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Kaban.GraphQL.MainTasks;
using Kaban.Models.Dto;
using Kaban.Tests.Setup;
using Snapshooter;
using Snapshooter.Xunit;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests.TestMainTask;

[Collection(nameof(WebAppCollectionFixture))]
public class TestMainTask(WebAppFactory factory, ITestOutputHelper testOutputHelper)
    : TestMainTaskBase(factory, testOutputHelper)
{
    [Fact]
    public async Task GraphQL_AddMainTask()
    {
        await ResetAndPopulateDb();

        {
            var response = await AddMainTask(HttpClientShadow);

            var jsonWrapped = await response.Content.ReadAsStringAsync();
            TestOutputHelper.WriteLine(jsonWrapped);

            JsonSerializer
                .Serialize(JsonSerializer.Deserialize<object>(jsonWrapped), TestHelper.JsonSerializerOptions)
                .MatchSnapshot();
        }
        PrettyJson(await GetQueryBoardsString()).MatchSnapshot(SnapshotNameExtension.Create("FullBoardQuery"));
    }


    [Fact]
    public async Task GraphQL_DeleteMainTask()
    {
        await ResetAndPopulateDb();

        var id = 0;
        {
            var response = await AddMainTask(HttpClientShadow);
            var jsonWrapped = await response.Content.ReadAsStringAsync();
            TestOutputHelper.WriteLine(jsonWrapped);
            var jsonObject = JsonNode.Parse(jsonWrapped)!.AsObject();
            id = int.Parse(jsonObject["data"]!["addMainTask"]!["mainTask"]!["id"]!.ToString());
            TestOutputHelper.WriteLine($"{id}");
        }

        {
            await DeleteMainTask(HttpClientShadow, new DeleteMainTaskInput(id));

            var jsonWrapped = await GetQueryBoardsString();
            var json = JsonNode.Parse(jsonWrapped)!["data"]!["boards"]!.ToString();
            var boardsDto = JsonSerializer.Deserialize<List<BoardDto>>(json,
                new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            boardsDto![0].Columns[0]!.MainTasks.Should().NotContain(c => c.Id == id);
        }
    }


    [Fact]
    public async Task GraphQL_PatchMainTask()
    {
        await ResetAndPopulateDb();

        PrettyJson(await PatchMainTask(HttpClientShadow)).MatchSnapshot();
    }

    [Fact]
    public async Task GraphQL_MoveMainTask_DisplaceToOtherColumn()
    {
        await ResetAndPopulateDb();

        PrettyJson(await MoveMainTask(HttpClientShadow, new MoveMainTaskInput(1, "Doing", null)))
            .MatchSnapshot();
    }

    [Fact]
    public async Task GraphQL_MoveMainTask_WrongStatus()
    {
        await ResetAndPopulateDb();

        PrettyJson(await MoveMainTask(HttpClientShadow, new MoveMainTaskInput(1, "Wrong!", null),
                HttpStatusCode.InternalServerError))
            .MatchSnapshot();
    }

    [Fact]
    public async Task GraphQL_MoveMainTask_Order_Same_Negative()
    {
        await ResetAndPopulateForMainTasks();

        PrettyJson(await MoveMainTask(HttpClientShadow, new MoveMainTaskInput(1, null, -3),
                HttpStatusCode.InternalServerError))
            .MatchSnapshot();
    }

    [Fact]
    public async Task GraphQL_MoveMainTask_Order_Same_Over()
    {
        await ResetAndPopulateForMainTasks();

        PrettyJson(await MoveMainTask(HttpClientShadow, new MoveMainTaskInput(1, null, 4),
                HttpStatusCode.InternalServerError))
            .MatchSnapshot();
    }

    [Fact]
    public async Task GraphQL_MoveMainTask_Order_Same_Same()
    {
        await ResetAndPopulateForMainTasks();

        PrettyJson(await MoveMainTask(HttpClientShadow, new MoveMainTaskInput(1, null, 0)))
            .MatchSnapshot();
    }

    [Fact]
    public async Task GraphQL_MoveMainTask_Order_Same_0_3()
    {
        await ResetAndPopulateForMainTasks();

        PrettyJson(await MoveMainTask(HttpClientShadow, new MoveMainTaskInput(1, null, 3)))
            .MatchSnapshot();
    }

    [Fact]
    public async Task GraphQL_MoveMainTask_Order_Same_3_0()
    {
        await ResetAndPopulateForMainTasks();
    
        PrettyJson(await MoveMainTask(HttpClientShadow, new MoveMainTaskInput(4, null, 0)))
            .MatchSnapshot();
    }
    
    
    [Fact]
    public async Task GraphQL_MoveMainTask_Displace_Order_0()
    {
        await ResetAndPopulateForMainTasks();
    
        PrettyJson(await MoveMainTask(HttpClientShadow, new MoveMainTaskInput(3, "Column 1", 0)))
            .MatchSnapshot();
    }
    
    [Fact]
    public async Task GraphQL_MoveMainTask_Displace_Order_2()
    {
        await ResetAndPopulateForMainTasks();
    
        PrettyJson(await MoveMainTask(HttpClientShadow, new MoveMainTaskInput(3, "Column 1", 2)))
            .MatchSnapshot();
    }
    
    [Fact]
    public async Task GraphQL_MoveMainTask_Displace_Order_4()
    {
        await ResetAndPopulateForMainTasks();
    
        PrettyJson(await MoveMainTask(HttpClientShadow, new MoveMainTaskInput(3, "Column 1", 4)))
            .MatchSnapshot();
    }

    // [Fact]
    // public async Task GraphQL_MoveMainTask_DisplaceToOtherColumn_InsertTo_4()
}