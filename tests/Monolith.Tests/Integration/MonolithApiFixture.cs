using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monolith.Modules.Orders.Infrastructure.Persistence;
using Monolith.Modules.Users.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Monolith.Tests.Integration;

[CollectionDefinition("api")]
public sealed class MonolithApiCollection : ICollectionFixture<MonolithApiFixture>;

public sealed class MonolithApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("monolith_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _container.StartAsync();

        // Accessing Services triggers ConfigureWebHost and starts the test host.
        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<OrdersDbContext>().Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<UsersDbContext>().Database.EnsureCreatedAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        await _container.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _container.GetConnectionString()
            });
        });
    }
}
