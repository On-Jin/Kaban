using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using FluentAssertions;
using Kaban.Data;
using Kaban.GraphQL.Boards;
using Kaban.Tests.Setup;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.Xunit;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests;

[Collection(nameof(WebAppCollectionFixture))]
public class TestBoard
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly JsonSerializerOptions _jsonOptionInputGraphQl;
    private readonly HttpClient _httpClientDiscord;
    private readonly HttpClient _httpClientShadow;

    private readonly Func<Task> _resetDatabase;
    private readonly WebAppFactory _factory;

    public TestBoard(WebAppFactory factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _jsonOptionInputGraphQl = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        _jsonOptionInputGraphQl.Converters.Add(new JsonStringEnumConverter());


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

    private async Task<HttpResponseMessage> MakeGraphQL_Request(HttpClient httpClient, string pathToGqlQueryFile,
        string input = "", string? mutationName = null)
    {
        string jsonContent =
            await Utilities.FileGqlToJsonQuery(pathToGqlQueryFile, input.Replace("\\\"", "\""), mutationName);
        _testOutputHelper.WriteLine($"\\\\\\\\ Input");
        _testOutputHelper.WriteLine(jsonContent);
        _testOutputHelper.WriteLine($"\\\\\\\\");
        StringContent httpContent = new(jsonContent, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await httpClient.PostAsync("/graphql", httpContent);

        _testOutputHelper.WriteLine($"//// Response from Query from {pathToGqlQueryFile}");
        _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());
        _testOutputHelper.WriteLine("////");

        return response;
    }

    private async Task<HttpResponseMessage> QueryBoards(HttpClient httpClient)
    {
        HttpResponseMessage response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_Query_Boards.gql");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    private async Task<HttpResponseMessage> AddBoard(HttpClient httpClient, AddBoardInput? addBoardInput = null)
    {
        addBoardInput ??= new AddBoardInput("Example");

        HttpResponseMessage response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_AddBoard.gql",
            JsonSerializer.Serialize(addBoardInput, _jsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    private async Task<HttpResponseMessage> DeleteBoard(HttpClient httpClient,
        DeleteBoardInput? deleteBoardInput = null)
    {
        deleteBoardInput ??= new DeleteBoardInput(1);

        HttpResponseMessage response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_DeleteBoard.gql",
            JsonSerializer.Serialize(deleteBoardInput, _jsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    private async Task<HttpResponseMessage> PatchBoard(HttpClient httpClient,
        PatchBoardInput? patchBoardInput = null)
    {
        patchBoardInput ??= new PatchBoardInput(1, "New Name Example!");

        HttpResponseMessage response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_PatchBoard.gql",
            JsonSerializer.Serialize(patchBoardInput, _jsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    private async Task GenerateKabanBoardToShadowUser(AppDbContext db)
    {
        var user = (await db.Users.FindAsync(new Guid(TestHelper.GuidShadowAuth)))!;
        var boards = await GenerateHelper.GenerateDefaultKabanBoards();
        user.Boards.AddRange(boards);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GraphQL_AddBoard()
    {
        await _resetDatabase();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await GenerateKabanBoardToShadowUser(db);

        HttpResponseMessage response = await AddBoard(_httpClientShadow);

        string jsonWrapped = await response.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine(jsonWrapped);

        jsonWrapped.MatchSnapshot();
    }

    [Fact]
    public async Task GraphQL_DeleteBoard()
    {
        await _resetDatabase();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await GenerateKabanBoardToShadowUser(db);

        int id = 0;
        {
            var response = await AddBoard(_httpClientShadow);
            var jsonWrapped = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine(jsonWrapped);
            var jsonObject = JsonNode.Parse(jsonWrapped)!.AsObject();
            id = int.Parse(jsonObject["data"]!["addBoard"]!["board"]["id"]!.ToString());
            _testOutputHelper.WriteLine($"{id}");
        }

        {
            {
                await DeleteBoard(_httpClientShadow, new DeleteBoardInput(id));
            }
            {
                var response = await QueryBoards(_httpClientShadow);
                string jsonWrapped = await response.Content.ReadAsStringAsync();
                JsonSerializer
                    .Serialize(JsonSerializer.Deserialize<object>(jsonWrapped), TestHelper.JsonSerializerOptions)
                    .MatchSnapshot();
            }
        }
    }


    [Fact]
    public async Task GraphQL_PatchBoard()
    {
        await _resetDatabase();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await GenerateKabanBoardToShadowUser(db);

        var response = await PatchBoard(_httpClientShadow);
        JsonSerializer
            .Serialize(
                JsonSerializer.Deserialize<object>(await response.Content.ReadAsStringAsync()),
                TestHelper.JsonSerializerOptions)
            .MatchSnapshot();
    }
}