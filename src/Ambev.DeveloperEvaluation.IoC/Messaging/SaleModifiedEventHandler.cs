using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.IoC.Messaging;

/// <summary>
/// Handles SaleModifiedEvent messages received from the bus by logging them.
/// </summary>
public class SaleModifiedEventHandler : IHandleMessages<SaleModifiedEvent>
{
    private readonly ILogger<SaleModifiedEventHandler> _logger;

    public SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleModifiedEvent message)
    {
        _logger.LogInformation("Event {EventName} received: {@Event}", nameof(SaleModifiedEvent), message);
        return Task.CompletedTask;
    }
}
