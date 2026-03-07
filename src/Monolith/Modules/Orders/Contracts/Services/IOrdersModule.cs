using Monolith.Modules.Orders.Contracts.Dtos;
using Monolith.Modules.Orders.Contracts.Requests;

namespace Monolith.Modules.Orders.Contracts.Services;

/// <summary>
/// Public module boundary contract for the Orders module.
/// This is a gateway used by other modules, not an application service with business logic.
/// </summary>
public interface IOrdersModule
{
    /// <summary>
    /// Creates a new order in the Orders module.
    /// </summary>
    /// <param name="request">Input data required to place an order.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The identifier of the created order.</returns>
    Task<Guid> PlaceOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an order snapshot by its identifier.
    /// </summary>
    /// <param name="orderId">Order identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The order data, or <see langword="null"/> when not found.</returns>
    Task<OrderDto?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}
