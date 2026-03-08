using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monolith.Modules.Users.Domain.Entities;

namespace Monolith.Modules.Users.Infrastructure.Persistence;

public class DashboardConfiguration : IEntityTypeConfiguration<Dashboard>
{
    public void Configure(EntityTypeBuilder<Dashboard> builder)
    {
        builder.ToTable("Dashboards");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.TotalPurchasesAmount).IsRequired();
        builder.HasIndex(d => d.ConsolidatedOn).IsUnique();
    }
}
