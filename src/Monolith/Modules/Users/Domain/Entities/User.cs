using Monolith.BuildingBlocks.Domain;
using Monolith.Modules.Users.Domain.DomainEvents;

namespace Monolith.Modules.Users.Domain.Entities;

internal class User : AggregateRoot<Guid>
{
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private User() { }

    public static (User user, UserCreatedDomainEvent domainEvent) Create(string email, string fullName)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = fullName,
            CreatedAt = DateTime.UtcNow
        };

        return (user, new UserCreatedDomainEvent(user.Id, user.Email, user.FullName));
    }
}
