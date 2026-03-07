using Monolith.BuildingBlocks.Domain;
using Monolith.Modules.Orders.Domain.DomainEvents;
using Monolith.Modules.Orders.Domain.Enums;

namespace Monolith.Modules.Orders.Domain.Entities;

internal class Order : AggregateRoot<Guid>
{
    public string CustomerName { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Order() { }

    public static (Order order, OrderPlacedDomainEvent domainEvent) Create(string customerName, decimal totalAmount)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = customerName,
            TotalAmount = totalAmount,
            Status = OrderStatus.Placed,
            CreatedAt = DateTime.UtcNow
        };

        return (order, new OrderPlacedDomainEvent(order.Id, order.CustomerName, order.TotalAmount));
    }
}
