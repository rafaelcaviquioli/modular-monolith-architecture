using Monolith.Modules.Users.Contracts.Dtos;
using Monolith.Modules.Users.Contracts.Services;
using Monolith.Modules.Users.Features.CreateUser;
using Monolith.Modules.Users.Features.GetUser;
using Wolverine;

namespace Monolith.Modules.Users;

public class UsersModuleService(IMessageBus bus) : IUsersModule
{
    public Task<Guid> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default) =>
        bus.InvokeAsync<Guid>(new CreateUserCommand(dto.Email, dto.FullName), cancellationToken);

    public Task<GetUserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        bus.InvokeAsync<GetUserDto?>(new GetUserQuery(userId), cancellationToken);
}
