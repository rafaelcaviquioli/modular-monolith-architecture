using Monolith.Modules.Orders.Application.Commands.PlaceOrder;
using Monolith.Modules.Orders.Application.Queries.GetOrder;
using Monolith.Modules.Orders.Contracts.Dtos;
using Monolith.Modules.Orders.Contracts.Requests;
using Monolith.Modules.Orders.Contracts.Services;
using Wolverine;

namespace Monolith.Modules.Orders;

internal class OrdersModuleService(IMessageBus bus) : IOrdersModule
{
    public Task<Guid> PlaceOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default) =>
        bus.InvokeAsync<Guid>(new PlaceOrderCommand(request.CustomerName, request.TotalAmount), cancellationToken);

    public Task<OrderDto?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        bus.InvokeAsync<OrderDto?>(new GetOrderQuery(orderId), cancellationToken);
}
