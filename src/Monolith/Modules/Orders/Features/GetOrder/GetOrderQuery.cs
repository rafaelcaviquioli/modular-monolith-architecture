using Monolith.BuildingBlocks.Application;

namespace Monolith.Modules.Orders.Features.GetOrder;

public record GetOrderQuery(Guid OrderId) : IQuery<GetOrderResponse?>;
