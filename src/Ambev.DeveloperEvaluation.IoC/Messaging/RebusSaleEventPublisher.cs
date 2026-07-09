using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Events;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.IoC.Messaging;

/// <summary>
/// Publishes Sale domain events onto the Rebus bus. Application handlers depend
/// only on ISaleEventPublisher; this class is the concrete adapter to a specific
/// messaging technology, kept in the composition root (IoC) rather than in
/// Application, so the Application layer never needs to know Rebus exists.
/// </summary>
public class RebusSaleEventPublisher : ISaleEventPublisher
{
    private readonly IBus _bus;

    public RebusSaleEventPublisher(IBus bus)
    {
        _bus = bus;
    }

    public Task PublishSaleCreatedAsync(SaleCreatedEvent @event, CancellationToken cancellationToken = default)
        => _bus.Publish(@event);

    public Task PublishSaleModifiedAsync(SaleModifiedEvent @event, CancellationToken cancellationToken = default)
        => _bus.Publish(@event);

    public Task PublishSaleCancelledAsync(SaleCancelledEvent @event, CancellationToken cancellationToken = default)
        => _bus.Publish(@event);

    public Task PublishItemCancelledAsync(ItemCancelledEvent @event, CancellationToken cancellationToken = default)
        => _bus.Publish(@event);
}
