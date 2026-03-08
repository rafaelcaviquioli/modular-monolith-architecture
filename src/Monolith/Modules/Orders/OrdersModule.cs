using Microsoft.EntityFrameworkCore;
using Monolith.Modules.Orders.Contracts.Services;
using Monolith.Modules.Orders.Infrastructure.Persistence;

namespace Monolith.Modules.Orders;

public static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<OrdersDbContext>(options =>
            options.UseNpgsql(connectionString), optionsLifetime:ServiceLifetime.Singleton
        );

        services.AddScoped<IOrdersModule, OrdersModuleService>();

        return services;
    }
}
