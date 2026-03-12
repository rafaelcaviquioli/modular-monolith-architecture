using Microsoft.EntityFrameworkCore;
using Monolith.Modules.Orders.Infrastructure.Persistence;
using Monolith.Modules.Users.Infrastructure.Persistence;
using Xunit;

namespace Monolith.Tests.Integration;

/// <summary>
/// Verifies both module DbContexts can connect to the PostgreSQL Testcontainer.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
public class PostgreSqlConnectionTest(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task DbContexts_Can_Connect_To_PostgreSql()
    {
        var ordersOptions = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;

        await using var ordersDbContext = new OrdersDbContext(ordersOptions);

        var usersOptions = new DbContextOptionsBuilder<UsersDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;

        await using var usersDbContext = new UsersDbContext(usersOptions);

        Assert.True(await ordersDbContext.Database.CanConnectAsync());
        Assert.True(await usersDbContext.Database.CanConnectAsync());
    }
}
