using Microsoft.AspNetCore.Mvc;
using Monolith.Modules.Users.Application.Commands.CreateUser;
using Monolith.Modules.Users.Application.Queries.GetUser;
using Monolith.Modules.Users.Contracts.Dtos;
using Monolith.Modules.Users.Contracts.Requests;
using Wolverine;

namespace Monolith.Modules.Users.API;

[ApiController]
[Route("api/users")]
internal class UsersController(IMessageBus bus) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var userId = await bus.InvokeAsync<Guid>(new CreateUserCommand(request.Email, request.FullName));
        return CreatedAtAction(nameof(GetUser), new { id = userId }, new { id = userId });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await bus.InvokeAsync<UserDto?>(new GetUserQuery(id));
        return user is null ? NotFound() : Ok(user);
    }
}
