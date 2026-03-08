using Monolith.Modules.Orders.Contracts.IntegrationEvents;
using Monolith.Modules.Orders.Domain.DomainEvents;

namespace Monolith.Modules.Orders.DomainEventHandlers;

public class OrderPlacedDomainEventHandler
{
    public OrderPlacedIntegrationEvent Handle(OrderPlacedDomainEvent domainEvent)
    {
        return new(domainEvent.OrderId, domainEvent.CustomerName, domainEvent.TotalAmount, DateTime.UtcNow);
    }
}
