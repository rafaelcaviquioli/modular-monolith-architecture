using Microsoft.EntityFrameworkCore;
using Monolith.Modules.Users.Features.UpdateDashboard;
using Monolith.Modules.Users.Infrastructure.Persistence;
using Xunit;

namespace Monolith.Tests.Integration.Users;

[Collection(PostgreSqlCollection.Name)]
public class UpdateDashboardCommandHandlerTests
{
    private readonly PostgreSqlFixture _fixture = null!;
    private readonly UsersDbContext _dbContext;

    public UpdateDashboardCommandHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;

        var usersOptions = new DbContextOptionsBuilder<UsersDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        _dbContext = new UsersDbContext(usersOptions);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        Assert.True(_dbContext.Database.CanConnect());
    }

    [Fact]
    public async Task UpdateDashboardCommandHandler_WhenNoDashboardExistsForDate_CreatesNewDashboardRecord()
    {
        var command = new UpdateDashboardCommand(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), 100);
        var handler = new UpdateDashboardCommandHandler(_dbContext);
        await handler.HandleAsync(command);
        var dashboard = await _dbContext.Dashboards.FirstOrDefaultAsync(d => d.ConsolidatedOn == command.Date);
        Assert.NotNull(dashboard);
        Assert.Equal(command.OrderAmount, dashboard.TotalPurchasesAmount);
    }

    [Fact]
    public async Task UpdateDashboardCommandHandler_WhenPurchasesInMultipleDays_MaintainSeparateRecordsPerConsolidationDate()
    {
        // yesterday purchases
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var handler = new UpdateDashboardCommandHandler(_dbContext);
        await handler.HandleAsync(new UpdateDashboardCommand(yesterday, 7500));
        await handler.HandleAsync(new UpdateDashboardCommand(yesterday, 10000));

        // today purchases
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var handlerToday = new UpdateDashboardCommandHandler(_dbContext);
        await handlerToday.HandleAsync(new UpdateDashboardCommand(today, 1500));
        await handlerToday.HandleAsync(new UpdateDashboardCommand(today, 1000));

        // detach entities to ensure we are reading the updated values from the database
        _dbContext.ChangeTracker.Clear();
        
        // assert only 2 records exist
        Assert.Equal(2, await _dbContext.Dashboards.CountAsync());

        // assert yesterday dashboard
        var yesterdayDashboard = await _dbContext.Dashboards.FirstOrDefaultAsync(d => d.ConsolidatedOn == yesterday);
        Assert.NotNull(yesterdayDashboard);
        Assert.Equal(17500, yesterdayDashboard.TotalPurchasesAmount);

        // assert today dashboard
        var todayDashboard = await _dbContext.Dashboards.FirstOrDefaultAsync(d => d.ConsolidatedOn == today);
        Assert.NotNull(todayDashboard);
        Assert.Equal(2500, todayDashboard.TotalPurchasesAmount);
    }
}
