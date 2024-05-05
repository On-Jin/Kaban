// using Kaban.Data;
// using Kaban.Model;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using Testcontainers.PostgreSql;
//
// namespace Kaban.Tests;
//
// [CollectionDefinition(nameof(WebAppCollection))]
// public class WebAppCollection : ICollectionFixture<WebAppFixture>;
//
// public class WebAppFixture : IAsyncLifetime
// {
//     private readonly PostgreSqlContainer _container =
//         new PostgreSqlBuilder()
//             .Build();
//
//     public async Task InitializeAsync()
//     {
//         await _container.StartAsync();
//
//         AppDbContext appDbContext = new AppDbContext(new DbContextOptions<AppDbContext>() { });
//         _factory = new WebApplicationFactory<Program>()
//             .WithWebHostBuilder(host =>
//             {
//                 host.UseSetting(
//                     "ConnectionStrings:KabanDbConnectionString",
//                     _container.GetConnectionString()
//                 );
//             });
//     }
//
//     public Task DisposeAsync()
//         => _container.DisposeAsync().AsTask();
// }