﻿using System.Data.Common;
using Kaban.Data;
using Kaban.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        builder.ConfigureTestServices(services =>
        {
            services.Configure<TestAuthHandlerOptions>(options => options.DefaultUserId = "1");

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                })
                .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme,
                    options => { });
        });

        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });

        var configurationValues = new Dictionary<string, string>
        {
            { "DISCORD_CLIENT_ID", "dummy" },
            { "DISCORD_SECRET", "dummy" },
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();
        builder.UseConfiguration(configuration)
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddInMemoryCollection(configurationValues);
            });
        base.ConfigureWebHost(builder);
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
        await ReseedIdentityColumnsAsync();
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

    private async Task ReseedIdentityColumnsAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tablesToReseed = new[] { "Boards", "Columns", "MainTasks", "SubTasks" };

        foreach (var tableName in tablesToReseed)
        {
#pragma warning disable EF1002
            await db.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{tableName}\" RESTART IDENTITY CASCADE;");
#pragma warning restore EF1002
        }
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

        {
            var user = new User()
            {
                Id = new Guid(TestHelper.GuidDiscordAuth),
                DiscordId = "159484228503624441",
                DiscordAvatar = "gyfujnokygujokmplyhjnk",
                DiscordUsername = "qwuiewou"
            };
            db.Users.Add(user);
        }
        {
            var user = new User()
            {
                Id = new Guid(TestHelper.GuidShadowAuth),
            };
            db.Users.Add(user);
        }
        await db.SaveChangesAsync();
    }


    public new async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}