using Monolith.BuildingBlocks.Application;
using Monolith.Modules.Users.Contracts.Dtos;

namespace Monolith.Modules.Users.Application.Queries.GetUser;

internal record GetUserQuery(Guid UserId) : IQuery<UserDto?>;
