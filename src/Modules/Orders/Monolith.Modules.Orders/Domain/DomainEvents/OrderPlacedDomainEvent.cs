using Monolith.BuildingBlocks.Domain;

namespace Monolith.Modules.Orders.Domain.DomainEvents;

internal record OrderPlacedDomainEvent(Guid OrderId, string CustomerName, decimal TotalAmount): IDomainEvent;
