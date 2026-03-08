namespace Monolith.Modules.Orders.Features.GetOrder;

public record GetOrderResponse(Guid Id, string CustomerName, decimal TotalAmount, string Status);
