using Monolith.BuildingBlocks.Application;

namespace Monolith.Modules.Users.Application.Commands.CreateUser;

internal record CreateUserCommand(string Email, string FullName) : ICommand<Guid>;
