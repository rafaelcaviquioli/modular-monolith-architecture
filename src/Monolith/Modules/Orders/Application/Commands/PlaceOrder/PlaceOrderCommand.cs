using Monolith.BuildingBlocks.Application;

namespace Monolith.Modules.Orders.Application.Commands.PlaceOrder;

public record PlaceOrderCommand(string CustomerName, decimal TotalAmount) : ICommand<Guid>;
