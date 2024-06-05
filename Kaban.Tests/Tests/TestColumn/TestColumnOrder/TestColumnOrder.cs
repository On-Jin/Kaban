using System.Net;
using Kaban.GraphQL.Columns;
using Kaban.Tests.Setup;
using Snapshooter;
using Snapshooter.Xunit;
using Xunit.Abstractions;

namespace Kaban.Tests.Tests.TestColumn.TestColumnOrder;

[Collection(nameof(WebAppCollectionFixture))]
public class TestColumnOrder(WebAppFactory factory, ITestOutputHelper testOutputHelper)
    : TestColumnBase(factory, testOutputHelper)
{
    [Fact]
    public async Task GraphQL_PatchColumn_Index_Negative()
    {
        await ResetAndPopulateForColumnOrder();
        var json = await PatchColumn(HttpClientShadow,
            new PatchColumnInput(1, null, -2),
            HttpStatusCode.InternalServerError);
        PrettyJson(json).MatchSnapshot();
    }

    [Fact]
    public async Task GraphQL_PatchColumn_Index_Over()
    {
        await ResetAndPopulateForColumnOrder();
        var json = await PatchColumn(HttpClientShadow,
            new PatchColumnInput(1, null, 9),
            HttpStatusCode.InternalServerError);
        PrettyJson(json).MatchSnapshot();
    }

    [Fact]
    public async Task GraphQL_PatchColumn_Index_0_to_0()
    {
        await ResetAndPopulateForColumnOrder();
        var json = await PatchColumn(HttpClientShadow,
            new PatchColumnInput(1, null, 0));
        PrettyJson(json).MatchSnapshot();
    }

    [Fact]
    public async Task GraphQL_PatchColumn_Index_0_to_2()
    {
        await ResetAndPopulateForColumnOrder();
        var json = await PatchColumn(HttpClientShadow,
            new PatchColumnInput(1, null, 2));
        PrettyJson(json).MatchSnapshot();
        (await GetQueryBoardsString()).MatchSnapshot(SnapshotNameExtension.Create("FullBoardQuery"));
    }

    [Fact]
    public async Task GraphQL_PatchColumn_Index_1_to_4()
    {
        await ResetAndPopulateForColumnOrder();
        var json = await PatchColumn(HttpClientShadow,
            new PatchColumnInput(2, null, 4));
        PrettyJson(json).MatchSnapshot();
        (await GetQueryBoardsString()).MatchSnapshot(SnapshotNameExtension.Create("FullBoardQuery"));
    }

    [Fact]
    public async Task GraphQL_PatchColumn_Index_8_to_2()
    {
        await ResetAndPopulateForColumnOrder();
        var json = await PatchColumn(HttpClientShadow,
            new PatchColumnInput(8, null, 2));
        PrettyJson(json).MatchSnapshot();
        (await GetQueryBoardsString()).MatchSnapshot(SnapshotNameExtension.Create("FullBoardQuery"));
    }
    
    [Fact]
    public async Task GraphQL_PatchColumn_Index_8_to_0()
    {
        await ResetAndPopulateForColumnOrder();
        var json = await PatchColumn(HttpClientShadow,
            new PatchColumnInput(8, null, 0));
        PrettyJson(json).MatchSnapshot();
        (await GetQueryBoardsString()).MatchSnapshot(SnapshotNameExtension.Create("FullBoardQuery"));
    }
    
    [Fact]
    public async Task GraphQL_PatchColumn_Index_0_to_7()
    {
        await ResetAndPopulateForColumnOrder();
        var json = await PatchColumn(HttpClientShadow,
            new PatchColumnInput(1, null, 7));
        PrettyJson(json).MatchSnapshot();
        (await GetQueryBoardsString()).MatchSnapshot(SnapshotNameExtension.Create("FullBoardQuery"));
    }
}