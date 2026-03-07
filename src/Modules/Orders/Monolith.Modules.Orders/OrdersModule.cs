using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monolith.Modules.Orders.Contracts.Services;
using Monolith.Modules.Orders.Infrastructure.Persistence;
using Wolverine;

namespace Monolith.Modules.Orders;

public static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrdersDbContext>(
            options => options.UseInMemoryDatabase("OrdersDb"),
            optionsLifetime: ServiceLifetime.Singleton
        );
        services.AddScoped<IOrdersModule, OrdersModuleService>();
        services.ConfigureWolverine(opts => opts.Discovery.IncludeAssembly(typeof(OrdersModule).Assembly));

        return services;
    }
}