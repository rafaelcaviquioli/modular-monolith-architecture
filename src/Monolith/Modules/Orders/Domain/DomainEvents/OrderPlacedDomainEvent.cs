using Monolith.BuildingBlocks.Domain;

namespace Monolith.Modules.Orders.Domain.DomainEvents;

public record OrderPlacedDomainEvent(Guid OrderId, string CustomerName, decimal TotalAmount) : IDomainEvent;
