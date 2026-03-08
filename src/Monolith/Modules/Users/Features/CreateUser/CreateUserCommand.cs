using Monolith.BuildingBlocks.Application;

namespace Monolith.Modules.Users.Features.CreateUser;

public record CreateUserCommand(string Email, string FullName) : ICommand<Guid>;
