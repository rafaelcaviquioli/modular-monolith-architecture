using Monolith.BuildingBlocks.Application;

namespace Monolith.Modules.Users.Features.UpdateDashboard;

public record UpdateDashboardCommand(DateOnly Date, decimal OrderAmount) : ICommand;
