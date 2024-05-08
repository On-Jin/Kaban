using System.Data.Common;
using Kaban.Data;
using Kaban.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Kaban.Tests.Setup;

public class WebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder()
            .Build();

    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(
            "ConnectionStrings:KabanDbConnectionString",
            _container.GetConnectionString());

        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
        base.ConfigureWebHost(builder);
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
        await PopulateDb();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await PopulateDb();
        _dbConnection = new NpgsqlConnection(_container.GetConnectionString());
        await InitializeRespawner();
    }

    private async Task InitializeRespawner()
    {
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    private async Task PopulateDb()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var author = new Author
        {
            Name = "Samia Kirk",
            Books =
            [
                new Book
                {
                    Title = "Sand"
                },

                new Book
                {
                    Title = "Fire"
                }
            ]
        };
        db.Authors.Add(author);
        await db.SaveChangesAsync();
    }


    public new async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}