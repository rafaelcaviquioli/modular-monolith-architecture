namespace Monolith.Modules.Orders.Contracts.Requests;

public record CreateOrderRequest(string CustomerName, decimal TotalAmount);
