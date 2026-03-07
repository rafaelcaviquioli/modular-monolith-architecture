namespace Monolith.Modules.Users.Contracts.IntegrationEvents;

public record UserCreatedIntegrationEvent(
    Guid UserId,
    string Email,
    string FullName,
    DateTime OccurredOn);
