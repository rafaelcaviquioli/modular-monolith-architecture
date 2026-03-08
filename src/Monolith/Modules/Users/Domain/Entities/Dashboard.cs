using Monolith.BuildingBlocks.Domain;

namespace Monolith.Modules.Users.Domain.Entities;

public class Dashboard : AggregateRoot<Guid>
{
    public decimal TotalPurchasesAmount { get; private set; }
    public DateOnly ConsolidatedOn { get; private set; }
    public DateTime ModifiedOn { get; private set; }

    public Dashboard()
    {
    }

    public static Dashboard Create(DateOnly consolidatedOn, decimal totalPurchasesAmount) => new()
    {
        Id = Guid.NewGuid(),
        ModifiedOn = DateTime.UtcNow,
        TotalPurchasesAmount = totalPurchasesAmount,
        ConsolidatedOn = consolidatedOn
    };

    public void Update(decimal orderAmount)
    {
        TotalPurchasesAmount += orderAmount;
        ModifiedOn = DateTime.UtcNow;
    }
}