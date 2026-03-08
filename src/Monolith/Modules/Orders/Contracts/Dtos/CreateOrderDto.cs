namespace Monolith.Modules.Orders.Contracts.Dtos;

public record CreateOrderDto(string CustomerName, decimal TotalAmount);
