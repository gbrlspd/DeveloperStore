using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Temporary ISaleEventPublisher implementation that just logs each event.
/// Stands in until the events are wired to a real bus (Rebus, in-memory
/// transport) so every Sale use case can already raise its event today.
/// </summary>
public class LoggingSaleEventPublisher : ISaleEventPublisher
{
    private readonly ILogger<LoggingSaleEventPublisher> _logger;

    public LoggingSaleEventPublisher(ILogger<LoggingSaleEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishSaleCreatedAsync(SaleCreatedEvent @event, CancellationToken cancellationToken = default)
        => LogEvent(nameof(SaleCreatedEvent), @event);

    public Task PublishSaleModifiedAsync(SaleModifiedEvent @event, CancellationToken cancellationToken = default)
        => LogEvent(nameof(SaleModifiedEvent), @event);

    public Task PublishSaleCancelledAsync(SaleCancelledEvent @event, CancellationToken cancellationToken = default)
        => LogEvent(nameof(SaleCancelledEvent), @event);

    public Task PublishItemCancelledAsync(ItemCancelledEvent @event, CancellationToken cancellationToken = default)
        => LogEvent(nameof(ItemCancelledEvent), @event);

    private Task LogEvent(string eventName, object payload)
    {
        _logger.LogInformation("Event {EventName} published: {@Payload}", eventName, payload);
        return Task.CompletedTask;
    }
}
