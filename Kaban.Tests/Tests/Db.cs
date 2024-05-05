using Kaban.Data;
using Kaban.Tests.Setup;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter;
using Snapshooter.Xunit;

namespace Kaban.Tests.Tests;

[Collection(nameof(WebAppCollectionFixture))]
public class Db : IAsyncLifetime
{
    private readonly AppDbContext _db;

    private readonly Func<Task> _resetDatabase;


    public Db(WebAppFactory factory)
    {
        _resetDatabase = factory.ResetDatabaseAsync;

        var serviceScope = factory.Services.CreateScope();

        _db = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _resetDatabase();

    [Fact]
    public async Task Db_Delete_First_Author()
    {
        await _resetDatabase();

        var f = _db.Authors.First();
        _db.Authors.Remove(f);
        await _db.SaveChangesAsync();

        _db.Authors.ToList().MatchSnapshot(SnapshotNameExtension.Create("Authors"));
        _db.Books.ToList().MatchSnapshot(SnapshotNameExtension.Create("Books"));
    }

    [Fact]
    public async Task Db_Delete_First_Book()
    {
        await _resetDatabase();

        var f = _db.Books.First();
        _db.Books.Remove(f);
        await _db.SaveChangesAsync();

        _db.Authors.ToList().MatchSnapshot(SnapshotNameExtension.Create("Authors"));
        _db.Books.ToList().MatchSnapshot(SnapshotNameExtension.Create("Books"));
    }
}