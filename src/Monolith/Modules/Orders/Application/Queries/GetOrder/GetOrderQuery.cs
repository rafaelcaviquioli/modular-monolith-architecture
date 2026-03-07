using Monolith.BuildingBlocks.Application;
using Monolith.Modules.Orders.Contracts.Dtos;

namespace Monolith.Modules.Orders.Application.Queries.GetOrder;

internal record GetOrderQuery(Guid OrderId) : IQuery<OrderDto?>;
