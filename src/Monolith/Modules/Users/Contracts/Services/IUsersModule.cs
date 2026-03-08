using Monolith.Modules.Users.Contracts.Dtos;

namespace Monolith.Modules.Users.Contracts.Services;

/// <summary>
/// Public module boundary contract for the Users module.
/// This is a gateway used by other modules, not an application service with business logic.
/// </summary>
public interface IUsersModule
{
    /// <summary>
    /// Creates a new user in the Users module.
    /// </summary>
    /// <param name="dto">Input data required to create a user.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The identifier of the created user.</returns>
    Task<Guid> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user snapshot by its identifier.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The user data, or <see langword="null"/> when not found.</returns>
    Task<GetUserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
