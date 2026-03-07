using Monolith.Modules.Orders.Domain.Entities;
using Monolith.Modules.Orders.Infrastructure.Persistence;
using Wolverine;

namespace Monolith.Modules.Orders.Application.Commands.PlaceOrder;

internal class PlaceOrderCommandHandler(OrdersDbContext dbContext)
{
    public (Guid, OutgoingMessages) Handle(PlaceOrderCommand command)
    {
        var (order, domainEvent) = Order.Create(command.CustomerName, command.TotalAmount);
        dbContext.Orders.Add(order);

        var messages = new OutgoingMessages { domainEvent };

        return (order.Id, messages);
    }
}
