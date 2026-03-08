using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Monolith.Modules.Orders.Features.GetOrder;

[ApiController]
[Route("api/orders")]
public class GetOrderController(IMessageBus bus) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetOrderResponse>> GetOrder(Guid id)
    {
        var order = await bus.InvokeAsync<GetOrderResponse?>(new GetOrderQuery(id));
        return order is null ? NotFound() : Ok(order);
    }
}
