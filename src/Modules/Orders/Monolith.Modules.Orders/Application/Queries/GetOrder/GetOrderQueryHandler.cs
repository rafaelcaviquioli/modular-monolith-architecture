using Microsoft.EntityFrameworkCore;
using Monolith.Modules.Orders.Contracts.Dtos;
using Monolith.Modules.Orders.Infrastructure.Persistence;

namespace Monolith.Modules.Orders.Application.Queries.GetOrder;

internal class GetOrderQueryHandler(OrdersDbContext dbContext)
{
    public async Task<OrderDto?> HandleAsync(GetOrderQuery query)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == query.OrderId);

        if (order is null) return null;

        return new OrderDto(order.Id, order.CustomerName, order.TotalAmount, order.Status.ToString());
    }
}
