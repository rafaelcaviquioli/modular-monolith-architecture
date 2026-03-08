using Monolith.Modules.Orders.Contracts.Dtos;
using Monolith.Modules.Orders.Contracts.Services;
using Monolith.Modules.Orders.Features.GetOrder;
using Monolith.Modules.Orders.Features.PlaceOrder;
using Wolverine;

namespace Monolith.Modules.Orders;

public class OrdersModuleService(IMessageBus bus) : IOrdersModule
{
    public Task<Guid> PlaceOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken = default) =>
        bus.InvokeAsync<Guid>(new PlaceOrderCommand(dto.CustomerName, dto.TotalAmount), cancellationToken);

    public async Task<GetOrderDto?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var response = await bus.InvokeAsync<GetOrderResponse?>(new GetOrderQuery(orderId), cancellationToken);
        if (response is null) return null;
        return new GetOrderDto(response.Id, response.CustomerName, response.TotalAmount, response.Status);
    }
}
