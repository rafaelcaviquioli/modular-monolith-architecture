using Microsoft.EntityFrameworkCore;
using Monolith.Modules.Users.Domain.Entities;
using Monolith.Modules.Users.Infrastructure.Persistence.Configurations;

namespace Monolith.Modules.Users.Infrastructure.Persistence;

internal class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("users");
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
