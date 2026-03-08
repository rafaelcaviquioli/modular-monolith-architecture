using Microsoft.AspNetCore.Mvc;
using Monolith.Modules.Orders.Contracts.Dtos;
using Wolverine;

namespace Monolith.Modules.Orders.Features.PlaceOrder;

[ApiController]
[Route("api/orders")]
public class PlaceOrderController(IMessageBus bus) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderDto dto)
    {
        var orderId = await bus.InvokeAsync<Guid>(new PlaceOrderCommand(dto.CustomerName, dto.TotalAmount));
        return Ok(new { id = orderId });
    }
}
