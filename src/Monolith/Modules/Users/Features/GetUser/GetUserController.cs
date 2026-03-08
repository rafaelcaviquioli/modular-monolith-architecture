using Microsoft.AspNetCore.Mvc;
using Monolith.Modules.Users.Contracts.Dtos;
using Wolverine;

namespace Monolith.Modules.Users.Features.GetUser;

[ApiController]
[Route("api/users")]
public class GetUserController(IMessageBus bus) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetUserDto>> GetUser(Guid id)
    {
        var user = await bus.InvokeAsync<GetUserDto?>(new GetUserQuery(id));
        return user is null ? NotFound() : Ok(user);
    }
}
