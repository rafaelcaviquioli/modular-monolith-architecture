using Monolith.BuildingBlocks.Domain;

namespace Monolith.Modules.Users.Domain.DomainEvents;

public record UserCreatedDomainEvent(Guid UserId, string Email, string FullName) : IDomainEvent;