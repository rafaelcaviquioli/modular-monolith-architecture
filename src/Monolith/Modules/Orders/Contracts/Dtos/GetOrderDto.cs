namespace Monolith.Modules.Orders.Contracts.Dtos;

public record GetOrderDto(Guid Id, string CustomerName, decimal TotalAmount, string Status);
