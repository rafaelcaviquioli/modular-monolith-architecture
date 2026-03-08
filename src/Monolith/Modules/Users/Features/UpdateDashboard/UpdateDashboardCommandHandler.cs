using Microsoft.EntityFrameworkCore;
using Monolith.Modules.Users.Domain.Entities;
using Monolith.Modules.Users.Infrastructure.Persistence;

namespace Monolith.Modules.Users.Features.UpdateDashboard;

public class UpdateDashboardCommandHandler(UsersDbContext dbContext)
{
    public async Task HandleAsync(UpdateDashboardCommand command, CancellationToken ct = default)
    {
        var dashboard = await dbContext.Dashboards.FirstOrDefaultAsync(d => d.ConsolidatedOn == command.Date, ct);
        
        if (dashboard is null)
        {
            dashboard = Dashboard.Create(command.Date, command.OrderAmount);
            dbContext.Dashboards.Add(dashboard);
            await dbContext.SaveChangesAsync(ct);
            return;
        }

        // Use ExecuteUpdate to avoid concurrency issues when multiple updates happen for the same consolidation date
        await dbContext.Dashboards
            .Where(d => d.ConsolidatedOn == command.Date)
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.TotalPurchasesAmount, d => d.TotalPurchasesAmount + command.OrderAmount), ct);
    }
}