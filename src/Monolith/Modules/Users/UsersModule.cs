using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monolith.Modules.Users.Contracts.Services;
using Monolith.Modules.Users.Infrastructure.Persistence;

namespace Monolith.Modules.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<UsersDbContext>(options =>
            options.UseInMemoryDatabase("UsersDb"),
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddScoped<IUsersModule, UsersModuleService>();

        return services;
    }
}
