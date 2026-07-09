using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.IoC.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

/// <summary>
/// Wires Rebus, with an in-memory transport, to publish and handle the Sale
/// domain events. This is intentionally not a real message broker: the
/// challenge only asks for the events to be raised and observable (each
/// handler just logs what it received), not actually delivered externally.
/// Swapping to a real broker (Azure Service Bus, RabbitMQ, ...) later is
/// purely a change to the .Transport(...) call below -- publishers,
/// handlers and every Application use case stay exactly the same.
/// </summary>
public class MessagingModuleInitializer : IModuleInitializer
{
    private static readonly InMemNetwork Network = new();
    private const string QueueName = "sales-events";

    public void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddRebus(configure => configure
            .Transport(t => t.UseInMemoryTransport(Network, QueueName))
            .Routing(r => r.TypeBased()
                .Map<SaleCreatedEvent>(QueueName)
                .Map<SaleModifiedEvent>(QueueName)
                .Map<SaleCancelledEvent>(QueueName)
                .Map<ItemCancelledEvent>(QueueName)));

        builder.Services.AutoRegisterHandlersFromAssemblyOf<SaleCreatedEventHandler>();

        builder.Services.AddScoped<ISaleEventPublisher, RebusSaleEventPublisher>();
    }
}
