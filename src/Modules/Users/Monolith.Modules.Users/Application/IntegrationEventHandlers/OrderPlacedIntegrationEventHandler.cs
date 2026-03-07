using Microsoft.Extensions.Logging;
using Monolith.Modules.Orders.Contracts.IntegrationEvents;

namespace Monolith.Modules.Users.Application.IntegrationEventHandlers;

/// <summary>
/// Example of cross-module async communication.
/// The Users module reacts to an event published by the Orders module
/// without any direct reference to Orders internals.
/// </summary>
internal class OrderPlacedIntegrationEventHandler(ILogger<OrderPlacedIntegrationEventHandler> logger)
{
    public Task HandleAsync(OrderPlacedIntegrationEvent integrationEvent)
    {
        logger.LogInformation(
            "Users module received OrderPlacedIntegrationEvent: Order {OrderId} placed by {CustomerName}",
            integrationEvent.OrderId,
            integrationEvent.CustomerName);

        // Add cross-module reaction logic here (e.g., reward points, notifications)
        return Task.CompletedTask;
    }
}
