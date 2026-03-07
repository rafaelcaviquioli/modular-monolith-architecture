using Microsoft.AspNetCore.Mvc;
using Monolith.Modules.Orders.Application.Commands.PlaceOrder;
using Monolith.Modules.Orders.Application.Queries.GetOrder;
using Monolith.Modules.Orders.Contracts.Dtos;
using Monolith.Modules.Orders.Contracts.Requests;
using Wolverine;

namespace Monolith.Modules.Orders.API;

[ApiController]
[Route("api/orders")]
internal class OrdersController(IMessageBus bus) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderRequest request)
    {
        var orderId = await bus.InvokeAsync<Guid>(new PlaceOrderCommand(request.CustomerName, request.TotalAmount));
        return CreatedAtAction(nameof(GetOrder), new { id = orderId }, new { id = orderId });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var order = await bus.InvokeAsync<OrderDto?>(new GetOrderQuery(id));
        return order is null ? NotFound() : Ok(order);
    }
}
