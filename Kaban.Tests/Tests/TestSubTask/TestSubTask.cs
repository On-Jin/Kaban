using System.Text.Json;
using Kaban.Tests.Setup;
using Snapshooter;
using Snapshooter.Xunit;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests.TestSubTask;

[Collection(nameof(WebAppCollectionFixture))]
public class TestSubTask(WebAppFactory factory, ITestOutputHelper testOutputHelper)
    : TestSubTaskBase(factory, testOutputHelper)
{
    [Fact]
    public async Task GraphQL_AddSubTask()
    {
        await ResetAndPopulateDb();

        {
            var response = await AddSubTask(HttpClientShadow);

            var jsonWrapped = await response.Content.ReadAsStringAsync();
            TestOutputHelper.WriteLine(jsonWrapped);

            JsonSerializer
                .Serialize(JsonSerializer.Deserialize<object>(jsonWrapped), TestHelper.JsonSerializerOptions)
                .MatchSnapshot();
        }
        PrettyJson(await GetQueryBoardsString()).MatchSnapshot(SnapshotNameExtension.Create("FullBoardQuery"));
    }
    
    [Fact]
    public async Task GraphQL_AddSubTasks()
    {
        await ResetAndPopulateDb();

        {
            var response = await AddSubTasks(HttpClientShadow);

            var jsonWrapped = await response.Content.ReadAsStringAsync();
            TestOutputHelper.WriteLine(jsonWrapped);

            JsonSerializer
                .Serialize(JsonSerializer.Deserialize<object>(jsonWrapped), TestHelper.JsonSerializerOptions)
                .MatchSnapshot();
        }
        PrettyJson(await GetQueryBoardsString()).MatchSnapshot(SnapshotNameExtension.Create("FullBoardQuery"));
    }
    
    [Fact]
    public async Task GraphQL_DeleteSubTask()
    {
        await ResetAndPopulateDb();

        {
            var response = await DeleteSubTask(HttpClientShadow);

            var jsonWrapped = await response.Content.ReadAsStringAsync();
            TestOutputHelper.WriteLine(jsonWrapped);

            JsonSerializer
                .Serialize(JsonSerializer.Deserialize<object>(jsonWrapped), TestHelper.JsonSerializerOptions)
                .MatchSnapshot();
        }
        PrettyJson(await GetQueryBoardsString()).MatchSnapshot(SnapshotNameExtension.Create("FullBoardQuery"));
    }
    
    [Fact]
    public async Task GraphQL_DeleteSubTasks()
    {
        await ResetAndPopulateDb();

        {
            var response = await DeleteSubTasks(HttpClientShadow);

            var jsonWrapped = await response.Content.ReadAsStringAsync();
            TestOutputHelper.WriteLine(jsonWrapped);

            JsonSerializer
                .Serialize(JsonSerializer.Deserialize<object>(jsonWrapped), TestHelper.JsonSerializerOptions)
                .MatchSnapshot();
        }
        PrettyJson(await GetQueryBoardsString()).MatchSnapshot(SnapshotNameExtension.Create("FullBoardQuery"));
    }
}