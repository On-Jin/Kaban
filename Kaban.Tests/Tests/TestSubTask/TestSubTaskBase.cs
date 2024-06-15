using System.Net;
using System.Text.Json;
using FluentAssertions;
using Kaban.Data;
using Kaban.GraphQL.MainTasks;
using Kaban.GraphQL.SubTasks;
using Kaban.Models;
using Kaban.Tests.Setup;
using Kaban.Tests.Tests.TestColumn;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests.TestSubTask;

public class TestSubTaskBase(WebAppFactory factory, ITestOutputHelper testOutputHelper)
    : TestColumnBase(factory, testOutputHelper)
{
    protected async Task<HttpResponseMessage> AddSubTask(HttpClient httpClient,
        AddSubTaskInput? addSubTaskInput = null)
    {
        addSubTaskInput ??= new AddSubTaskInput(1, "Sub Task Example");

        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_AddSubTask.gql",
            JsonSerializer.Serialize(addSubTaskInput, JsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    protected async Task<HttpResponseMessage> AddSubTasks(HttpClient httpClient,
        AddSubTasksInput? addSubTasksInput = null)
    {
        addSubTasksInput ??= new AddSubTasksInput(1, ["Sub Task Example 1", "Sub Task Example 2"]);

        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_AddSubTasks.gql",
            JsonSerializer.Serialize(addSubTasksInput, JsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    protected async Task<HttpResponseMessage> DeleteSubTask(HttpClient httpClient,
        DeleteSubTaskInput? deleteSubTaskInput = null)
    {
        deleteSubTaskInput ??= new DeleteSubTaskInput(1);

        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_DeleteSubTask.gql",
            JsonSerializer.Serialize(deleteSubTaskInput, JsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    protected async Task<HttpResponseMessage> DeleteSubTasks(HttpClient httpClient,
        DeleteSubTasksInput? deleteSubTasksInput = null)
    {
        deleteSubTasksInput ??= new DeleteSubTasksInput([1, 2]);

        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_DeleteSubTasks.gql",
            JsonSerializer.Serialize(deleteSubTasksInput, JsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    // B
    //  C
    //   M * 4
    //    S 1 2 3 4
    //  C
    //   M * 4
    //    S 1 2 3 4
    protected async Task ResetAndPopulateForSubTasks()
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
                List<SubTask> subTasks = new List<SubTask>();
                for (int iSub = 0; iSub <= i; iSub++)
                {
                    subTasks.Add(new SubTask()
                    {
                        IsCompleted = (iSub % 2) == 0,
                        Title = $"S{iSub}___MT_{i}__C{columnIndex}"
                    });
                }

                boards[0].Columns[columnIndex].MainTasks.Add(new MainTask
                {
                    Order = i,
                    Title = $"MT_{i}__C{columnIndex}",
                    Description = "",
                    SubTasks = subTasks,
                });
            }
        }

        user.Boards.AddRange(boards);
        await db.SaveChangesAsync();
    }
}