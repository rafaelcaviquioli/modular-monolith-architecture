using Monolith.Modules.Orders.Contracts.IntegrationEvents;
using Monolith.Modules.Users.Features.UpdateDashboard;
using Wolverine;

namespace Monolith.Modules.Users.IntegrationEventHandlers;

/// <summary>
/// Example of cross-module async communication.
/// The Users module reacts to an event published by the Orders module
/// without any direct reference to Orders internals.
/// </summary>
public class UpdateDashboardOnOrderPlacedIntegrationEventHandler(ILogger<UpdateDashboardOnOrderPlacedIntegrationEventHandler> logger)
{
    // TODO: Find a better solution for this use case because it might generate a inconsistent result in case of event failure and retry due to race conditions.
    // Two events executed at the same time will obtain a empty dashboard state from the database, resulting in creating two dashboards records for the same day.
    public async Task HandleAsync(OrderPlacedIntegrationEvent @event, IMessageBus bus, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Users module received OrderPlacedIntegrationEvent: Order {OrderId} placed by {CustomerName}",
            @event.OrderId,
            @event.CustomerName);

        var command = new UpdateDashboardCommand(DateOnly.FromDateTime(DateTime.UtcNow), @event.TotalAmount);
        await bus.InvokeAsync(command, cancellationToken);
    }
}
