namespace Monolith.Modules.Orders.Contracts.Dtos;

public record OrderDto(Guid Id, string CustomerName, decimal TotalAmount, string Status);
