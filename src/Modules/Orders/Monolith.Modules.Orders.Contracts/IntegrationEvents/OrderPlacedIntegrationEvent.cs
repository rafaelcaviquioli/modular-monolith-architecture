namespace Monolith.Modules.Orders.Contracts.IntegrationEvents;

public record OrderPlacedIntegrationEvent(
    Guid OrderId,
    string CustomerName,
    decimal TotalAmount,
    DateTime OccurredOn);
