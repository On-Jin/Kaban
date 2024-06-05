using System.Net;
using System.Text.Json;
using FluentAssertions;
using Kaban.Data;
using Kaban.GraphQL.MainTasks;
using Kaban.Models;
using Kaban.Tests.Setup;
using Kaban.Tests.Tests.TestColumn;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests.TestMainTask;

public class TestMainTaskBase(WebAppFactory factory, ITestOutputHelper testOutputHelper)
    : TestColumnBase(factory, testOutputHelper)
{
    protected async Task<HttpResponseMessage> AddMainTask(HttpClient httpClient,
        AddMainTaskInput? addMainTaskInput = null)
    {
        addMainTaskInput ??= new AddMainTaskInput(1, "Main Task Example", "Description");

        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_AddMainTask.gql",
            JsonSerializer.Serialize(addMainTaskInput, JsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    protected async Task<HttpResponseMessage> DeleteMainTask(HttpClient httpClient,
        DeleteMainTaskInput? deleteMainTaskInput = null)
    {
        deleteMainTaskInput ??= new DeleteMainTaskInput(1);

        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_DeleteMainTask.gql",
            JsonSerializer.Serialize(deleteMainTaskInput, JsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    protected async Task<string> PatchMainTask(HttpClient httpClient,
        PatchMainTaskInput? patchBoardInput = null,
        HttpStatusCode statusShould = HttpStatusCode.OK)
    {
        patchBoardInput ??= new PatchMainTaskInput(1, "New MainTask Name Example!", "Updated description!");

        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_PatchMainTask.gql",
            JsonSerializer.Serialize(patchBoardInput, JsonOptionInputGraphQl));
        response.StatusCode.Should().Be(statusShould);

        return await response.Content.ReadAsStringAsync();
    }

    protected async Task<string> MoveMainTask(HttpClient httpClient,
        MoveMainTaskInput moveBoardInput,
        HttpStatusCode statusShould = HttpStatusCode.OK)
    {
        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_MoveMainTask.gql",
            JsonSerializer.Serialize(moveBoardInput, JsonOptionInputGraphQl));
        response.StatusCode.Should().Be(statusShould);

        return await response.Content.ReadAsStringAsync();
    }

    // B
    //  C
    //   M * 4
    //  C
    //   M * 4
    protected async Task ResetAndPopulateForMainTasks()
    {
        await ResetDatabase();
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = (await db.Users.FindAsync(new Guid(TestHelper.GuidShadowAuth)))!;
        var boards = new List<Board>()
        {
            new()
            {
                Name = "Board Name",
                Columns = []
            }
        };

        for (var i = 0; i < 2; i++)
        {
            boards[0].Columns.Add(new Column()
            {
                Order = i,
                Name = $"Column {i}",
                MainTasks = []
            });
        }

        for (var columnIndex = 0; columnIndex < 2; columnIndex++)
        {
            for (var i = 0; i < 4; i++)
            {
                boards[0].Columns[columnIndex].MainTasks.Add(new MainTask
                {
                    Order = i,
                    Title = $"MT_{i}__C{columnIndex}",
                    Description = "",
                    SubTasks = [],
                });
            }
        }

        user.Boards.AddRange(boards);
        await db.SaveChangesAsync();
    }
}