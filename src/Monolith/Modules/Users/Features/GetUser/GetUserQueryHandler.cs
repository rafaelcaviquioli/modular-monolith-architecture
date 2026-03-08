using Microsoft.EntityFrameworkCore;
using Monolith.Modules.Users.Contracts.Dtos;
using Monolith.Modules.Users.Infrastructure.Persistence;

namespace Monolith.Modules.Users.Features.GetUser;

public class GetUserQueryHandler(UsersDbContext dbContext)
{
    public async Task<GetUserDto?> HandleAsync(GetUserQuery query)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.UserId);

        if (user is null) return null;

        return new GetUserDto(user.Id, user.Email, user.FullName);
    }
}
