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
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(connectionString), optionsLifetime:ServiceLifetime.Singleton
        );

        services.AddScoped<IUsersModule, UsersModuleService>();

        return services;
    }
}
