using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.IoC.Messaging;

/// <summary>
/// Handles SaleCreatedEvent messages received from the bus by logging them.
/// Stands in for a real subscriber (e.g. inventory, notifications) that would
/// react to a new sale in a production message-broker setup.
/// </summary>
public class SaleCreatedEventHandler : IHandleMessages<SaleCreatedEvent>
{
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    public SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCreatedEvent message)
    {
        _logger.LogInformation("Event {EventName} received: {@Event}", nameof(SaleCreatedEvent), message);
        return Task.CompletedTask;
    }
}
