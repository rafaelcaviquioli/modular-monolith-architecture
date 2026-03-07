using Monolith.Modules.Orders.Contracts.IntegrationEvents;
using Monolith.Modules.Orders.Domain.DomainEvents;

namespace Monolith.Modules.Orders.Application.DomainEventHandlers;

internal class OrderPlacedDomainEventHandler
{
    public OrderPlacedIntegrationEvent Handle(OrderPlacedDomainEvent domainEvent) =>
        new(domainEvent.OrderId, domainEvent.CustomerName, domainEvent.TotalAmount, DateTime.UtcNow);
}
