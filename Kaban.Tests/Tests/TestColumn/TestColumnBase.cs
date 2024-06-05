using System.Net;
using System.Text.Json;
using FluentAssertions;
using Kaban.Data;
using Kaban.GraphQL.Columns;
using Kaban.Models;
using Kaban.Tests.Setup;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests.TestColumn;

public class TestColumnBase(WebAppFactory factory, ITestOutputHelper testOutputHelper)
    : TestBase(factory, testOutputHelper)
{
    protected async Task<HttpResponseMessage> AddColumn(HttpClient httpClient, AddColumnInput? addColumnInput = null)
    {
        addColumnInput ??= new AddColumnInput(1, "Example");

        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_AddColumn.gql",
            JsonSerializer.Serialize(addColumnInput, JsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    protected async Task<HttpResponseMessage> DeleteColumn(HttpClient httpClient,
        DeleteColumnInput? deleteColumnInput = null)
    {
        deleteColumnInput ??= new DeleteColumnInput(1);

        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_DeleteColumn.gql",
            JsonSerializer.Serialize(deleteColumnInput, JsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    protected async Task<string> PatchColumn(HttpClient httpClient,
        PatchColumnInput? patchBoardInput = null,
        HttpStatusCode statusShould = HttpStatusCode.OK)
    {
        patchBoardInput ??= new PatchColumnInput(1, "New Column Name Example!");

        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_PatchColumn.gql",
            JsonSerializer.Serialize(patchBoardInput, JsonOptionInputGraphQl));
        response.StatusCode.Should().Be(statusShould);

        return await response.Content.ReadAsStringAsync();
    }

    protected async Task ResetAndPopulateForColumnOrder()
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

        for (var i = 0; i < 8; i++)
        {
            boards[0].Columns.Add(new Column()
            {
                Order = i,
                Name = $"Column {i}",
                MainTasks = []
            });
        }

        user.Boards.AddRange(boards);
        await db.SaveChangesAsync();
    }
}