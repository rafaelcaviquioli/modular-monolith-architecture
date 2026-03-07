using Monolith.Modules.Users.Application.Commands.CreateUser;
using Monolith.Modules.Users.Application.Queries.GetUser;
using Monolith.Modules.Users.Contracts.Dtos;
using Monolith.Modules.Users.Contracts.Requests;
using Monolith.Modules.Users.Contracts.Services;
using Wolverine;

namespace Monolith.Modules.Users;

internal class UsersModuleService(IMessageBus bus) : IUsersModule
{
    public Task<Guid> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default) =>
        bus.InvokeAsync<Guid>(new CreateUserCommand(request.Email, request.FullName), cancellationToken);

    public Task<UserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        bus.InvokeAsync<UserDto?>(new GetUserQuery(userId), cancellationToken);
}
