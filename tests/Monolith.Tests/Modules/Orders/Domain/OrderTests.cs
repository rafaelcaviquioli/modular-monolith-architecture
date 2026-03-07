using Monolith.Modules.Orders.Domain.DomainEvents;
using Monolith.Modules.Orders.Domain.Entities;
using Monolith.Modules.Orders.Domain.Enums;
using Xunit;

namespace Monolith.Tests.Modules.Orders.Domain;

public class OrderTests
{
    [Fact]
    public void Create_ShouldReturnOrderWithPlacedStatus()
    {
        var (order, _) = Order.Create("John Doe", 99.99m);

        Assert.Equal("John Doe", order.CustomerName);
        Assert.Equal(99.99m, order.TotalAmount);
        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.NotEqual(Guid.Empty, order.Id);
    }

    [Fact]
    public void Create_ShouldRaiseOrderPlacedDomainEvent()
    {
        var (order, domainEvent) = Order.Create("Jane Doe", 49.99m);

        Assert.Equal(order.Id, domainEvent.OrderId);
        Assert.Equal("Jane Doe", domainEvent.CustomerName);
        Assert.Equal(49.99m, domainEvent.TotalAmount);
    }
}
