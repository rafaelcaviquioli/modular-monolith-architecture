using Microsoft.EntityFrameworkCore;
using Monolith.Modules.Orders.Infrastructure.Persistence;

namespace Monolith.Modules.Orders.Features.GetOrder;

public class GetOrderQueryHandler(OrdersDbContext dbContext)
{
    public async Task<GetOrderResponse?> HandleAsync(GetOrderQuery query)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == query.OrderId);

        if (order is null)
            return null;

        return new GetOrderResponse(order.Id, order.CustomerName, order.TotalAmount, order.Status.ToString());
    }
}
