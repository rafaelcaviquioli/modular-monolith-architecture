using Monolith.BuildingBlocks.Application;
using Monolith.Modules.Users.Contracts.Dtos;

namespace Monolith.Modules.Users.Features.GetUser;

public record GetUserQuery(Guid UserId) : IQuery<GetUserDto?>;
