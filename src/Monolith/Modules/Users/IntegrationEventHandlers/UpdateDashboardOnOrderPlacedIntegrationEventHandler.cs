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
