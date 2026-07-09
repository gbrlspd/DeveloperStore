using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.IoC.Messaging;

/// <summary>
/// Handles SaleCancelledEvent messages received from the bus by logging them.
/// </summary>
public class SaleCancelledEventHandler : IHandleMessages<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledEventHandler> _logger;

    public SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCancelledEvent message)
    {
        _logger.LogInformation("Event {EventName} received: {@Event}", nameof(SaleCancelledEvent), message);
        return Task.CompletedTask;
    }
}
