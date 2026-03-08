using Monolith.Modules.Users.Domain.Entities;
using Monolith.Modules.Users.Infrastructure.Persistence;
using Wolverine;

namespace Monolith.Modules.Users.Features.CreateUser;

public class CreateUserCommandHandler(UsersDbContext dbContext)
{
    public (Guid, OutgoingMessages) Handle(CreateUserCommand command)
    {
        var (user, domainEvent) = User.Create(command.Email, command.FullName);
        dbContext.Users.Add(user);

        var messages = new OutgoingMessages { domainEvent };

        return (user.Id, messages);
    }
}
