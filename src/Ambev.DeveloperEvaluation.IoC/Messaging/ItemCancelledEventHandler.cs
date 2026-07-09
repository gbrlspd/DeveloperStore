using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.IoC.Messaging;

/// <summary>
/// Handles ItemCancelledEvent messages received from the bus by logging them.
/// </summary>
public class ItemCancelledEventHandler : IHandleMessages<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledEventHandler> _logger;

    public ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ItemCancelledEvent message)
    {
        _logger.LogInformation("Event {EventName} received: {@Event}", nameof(ItemCancelledEvent), message);
        return Task.CompletedTask;
    }
}
