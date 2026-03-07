using Monolith.Modules.Users.Contracts.IntegrationEvents;
using Monolith.Modules.Users.Domain.DomainEvents;

namespace Monolith.Modules.Users.Application.DomainEventHandlers;

internal class UserCreatedDomainEventHandler
{
    public UserCreatedIntegrationEvent Handle(UserCreatedDomainEvent domainEvent) =>
        new(domainEvent.UserId, domainEvent.Email, domainEvent.FullName, DateTime.UtcNow);
}
