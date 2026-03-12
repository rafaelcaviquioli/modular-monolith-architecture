using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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
    private string? _previousDefaultConnection;

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase($"monolith_tests_api_{Guid.NewGuid():N}")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _container.StartAsync();

        _previousDefaultConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _container.GetConnectionString());

        // Explicitly creating a client ensures the host lifecycle is initialized before resolving services.
        using var _ = CreateClient();

        await using var scope = Services.CreateAsyncScope();
        var ordersDbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        // Wolverine may create infrastructure tables first, so create module tables explicitly.
        var ordersDatabaseCreator = ordersDbContext.GetService<IRelationalDatabaseCreator>();
        await ordersDatabaseCreator.CreateTablesAsync();

        var usersDatabaseCreator = usersDbContext.GetService<IRelationalDatabaseCreator>();
        await usersDatabaseCreator.CreateTablesAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        await _container.DisposeAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _previousDefaultConnection);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _container.GetConnectionString()
            });
        });
    }
}
