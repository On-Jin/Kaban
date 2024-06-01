using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Kaban.Data;
using Kaban.GraphQL.Boards;
using Kaban.Tests.Setup;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests;

public abstract  class TestBase
{
    protected readonly ITestOutputHelper TestOutputHelper;
    protected readonly JsonSerializerOptions JsonOptionInputGraphQl;
    protected readonly HttpClient HttpClientDiscord;
    protected readonly HttpClient HttpClientShadow;

    protected readonly Func<Task> ResetDatabase;
    protected readonly WebAppFactory Factory;

    protected TestBase(WebAppFactory factory, ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
        JsonOptionInputGraphQl = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        JsonOptionInputGraphQl.Converters.Add(new JsonStringEnumConverter());


        Factory = factory;
        TestOutputHelper = testOutputHelper;
        ResetDatabase = factory.ResetDatabaseAsync;

        HttpClientDiscord = factory.CreateClient();
        HttpClientDiscord.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        HttpClientShadow = factory.CreateClient();

        HttpClientShadow.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        HttpClientShadow.DefaultRequestHeaders.Add(TestHelper.HeaderUserGuid, TestHelper.GuidShadowAuth);
    }

    protected async Task<HttpResponseMessage> MakeGraphQL_Request(HttpClient httpClient, string pathToGqlQueryFile,
        string input = "", string? mutationName = null)
    {
        var jsonContent =
            await Utilities.FileGqlToJsonQuery(pathToGqlQueryFile, input.Replace("\\\"", "\""), mutationName);
        TestOutputHelper.WriteLine($"\\\\\\\\ Input");
        TestOutputHelper.WriteLine(jsonContent);
        TestOutputHelper.WriteLine($"\\\\\\\\");
        StringContent httpContent = new(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("/graphql", httpContent);

        TestOutputHelper.WriteLine($"//// Response from Query from {pathToGqlQueryFile}");
        TestOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());
        TestOutputHelper.WriteLine("////");

        return response;
    }

    protected async Task<HttpResponseMessage> QueryBoards(HttpClient httpClient)
    {
        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_Query_Boards.gql");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    protected async Task<HttpResponseMessage> AddBoard(HttpClient httpClient, AddBoardInput? addBoardInput = null)
    {
        addBoardInput ??= new AddBoardInput("Example");

        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_AddBoard.gql",
            JsonSerializer.Serialize(addBoardInput, JsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    protected async Task<HttpResponseMessage> DeleteBoard(HttpClient httpClient,
        DeleteBoardInput? deleteBoardInput = null)
    {
        deleteBoardInput ??= new DeleteBoardInput(1);

        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_DeleteBoard.gql",
            JsonSerializer.Serialize(deleteBoardInput, JsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    protected async Task<HttpResponseMessage> PatchBoard(HttpClient httpClient,
        PatchBoardInput? patchBoardInput = null)
    {
        patchBoardInput ??= new PatchBoardInput(1, "New Name Example!");

        var response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_PatchBoard.gql",
            JsonSerializer.Serialize(patchBoardInput, JsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    protected async Task GenerateKabanBoardToShadowUser(AppDbContext db)
    {
        var user = (await db.Users.FindAsync(new Guid(TestHelper.GuidShadowAuth)))!;
        var boards = await GenerateHelper.GenerateDefaultKabanBoards();
        user.Boards.AddRange(boards);
        await db.SaveChangesAsync();
    }

    protected async Task<string> GetQueryBoardsString()
    {
        var response = await QueryBoards(HttpClientShadow);
        var jsonWrapped = await response.Content.ReadAsStringAsync();
        return JsonSerializer
            .Serialize(JsonSerializer.Deserialize<object>(jsonWrapped), TestHelper.JsonSerializerOptions);
    }

    protected async Task ResetAndPopulateDb()
    {
        await ResetDatabase();
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await GenerateKabanBoardToShadowUser(db);
    }

    protected string PrettyJson(string json)
    {
        return JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(json), TestHelper.JsonSerializerOptions);
    }
}