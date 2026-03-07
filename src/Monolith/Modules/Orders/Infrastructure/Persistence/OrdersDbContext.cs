using Microsoft.EntityFrameworkCore;
using Monolith.Modules.Orders.Domain.Entities;
using Monolith.Modules.Orders.Infrastructure.Persistence.Configurations;

namespace Monolith.Modules.Orders.Infrastructure.Persistence;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("orders");
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
    }
}
