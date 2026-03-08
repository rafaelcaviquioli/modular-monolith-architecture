using Microsoft.AspNetCore.Mvc;
using Monolith.Modules.Users.Contracts.Dtos;
using Wolverine;

namespace Monolith.Modules.Users.Features.CreateUser;

[ApiController]
[Route("api/users")]
public class CreateUserController(IMessageBus bus) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var userId = await bus.InvokeAsync<Guid>(new CreateUserCommand(dto.Email, dto.FullName));
        return Ok(new { id = userId });
    }
}
