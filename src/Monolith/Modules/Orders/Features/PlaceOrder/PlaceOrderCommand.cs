using Monolith.BuildingBlocks.Application;

namespace Monolith.Modules.Orders.Features.PlaceOrder;

public record PlaceOrderCommand(string CustomerName, decimal TotalAmount) : ICommand<Guid>;
