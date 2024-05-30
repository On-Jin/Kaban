﻿using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using FluentAssertions;
using Kaban.Data;
using Kaban.GraphQL.Boards;
using Kaban.GraphQL.Columns;
using Kaban.Tests.Setup;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter;
using Snapshooter.Xunit;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests;

public partial class TestBoard
{
    private async Task<HttpResponseMessage> AddColumn(HttpClient httpClient, AddColumnInput? addColumnInput = null)
    {
        addColumnInput ??= new AddColumnInput(1, "Example");

        HttpResponseMessage response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_AddColumn.gql",
            JsonSerializer.Serialize(addColumnInput, _jsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    private async Task<HttpResponseMessage> DeleteColumn(HttpClient httpClient,
        DeleteColumnInput? deleteColumnInput = null)
    {
        deleteColumnInput ??= new DeleteColumnInput(1);

        HttpResponseMessage response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_DeleteColumn.gql",
            JsonSerializer.Serialize(deleteColumnInput, _jsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }

    private async Task<HttpResponseMessage> PatchColumn(HttpClient httpClient,
        PatchColumnInput? patchBoardInput = null)
    {
        patchBoardInput ??= new PatchColumnInput(1, "New Column Name Example!");

        HttpResponseMessage response = await MakeGraphQL_Request(httpClient,
            "Resources/Ql_PatchColumn.gql",
            JsonSerializer.Serialize(patchBoardInput, _jsonOptionInputGraphQl));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return response;
    }


    [Fact]
    public async Task GraphQL_AddColumn()
    {
        await _resetDatabase();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await GenerateKabanBoardToShadowUser(db);

        {
            HttpResponseMessage response = await AddColumn(_httpClientShadow);

            string jsonWrapped = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine(jsonWrapped);

            JsonSerializer
                .Serialize(JsonSerializer.Deserialize<object>(jsonWrapped), TestHelper.JsonSerializerOptions)
                .MatchSnapshot();
        }
        (await GetQueryBoardsString()).MatchSnapshot(SnapshotNameExtension.Create("FullBoardQuery"));
    }

    [Fact]
    public async Task GraphQL_DeleteColumn()
    {
        await _resetDatabase();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await GenerateKabanBoardToShadowUser(db);

        int id = 0;
        {
            var response = await AddColumn(_httpClientShadow);
            var jsonWrapped = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine(jsonWrapped);
            var jsonObject = JsonNode.Parse(jsonWrapped)!.AsObject();
            id = int.Parse(jsonObject["data"]!["addColumn"]!["column"]["id"]!.ToString());
            _testOutputHelper.WriteLine($"{id}");
        }

        {
            var response = await DeleteColumn(_httpClientShadow, new DeleteColumnInput(id));
            string jsonWrapped = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine(jsonWrapped);
            JsonSerializer
                .Serialize(JsonSerializer.Deserialize<object>(jsonWrapped), TestHelper.JsonSerializerOptions)
                .MatchSnapshot();
            (await GetQueryBoardsString()).MatchSnapshot(SnapshotNameExtension.Create("FullBoardQuery"));
        }
    }


    [Fact]
    public async Task GraphQL_PatchColumn()
    {
        await _resetDatabase();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await GenerateKabanBoardToShadowUser(db);

        var response = await PatchColumn(_httpClientShadow);
        JsonSerializer
            .Serialize(
                JsonSerializer.Deserialize<object>(await response.Content.ReadAsStringAsync()),
                TestHelper.JsonSerializerOptions)
            .MatchSnapshot();
    }
}