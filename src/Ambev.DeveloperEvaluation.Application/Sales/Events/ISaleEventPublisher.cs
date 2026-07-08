using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Publishes Sale domain events. Application handlers depend on this
/// abstraction rather than on a specific message bus, so the actual
/// transport can be swapped in without touching any use case.
/// </summary>
public interface ISaleEventPublisher
{
    Task PublishSaleCreatedAsync(SaleCreatedEvent @event, CancellationToken cancellationToken = default);
    Task PublishSaleModifiedAsync(SaleModifiedEvent @event, CancellationToken cancellationToken = default);
    Task PublishSaleCancelledAsync(SaleCancelledEvent @event, CancellationToken cancellationToken = default);
    Task PublishItemCancelledAsync(ItemCancelledEvent @event, CancellationToken cancellationToken = default);
}
