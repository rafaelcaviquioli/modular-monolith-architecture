using Monolith.BuildingBlocks.Domain;

namespace Monolith.Modules.Users.Domain.DomainEvents;

internal record UserCreatedDomainEvent(
    Guid UserId,
    string Email,
    string FullName) : IDomainEvent;
