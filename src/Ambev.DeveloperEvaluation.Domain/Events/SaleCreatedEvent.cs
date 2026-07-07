namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Raised when a new sale is created.
/// </summary>
public sealed record SaleCreatedEvent(Guid SaleId, string SaleNumber, DateTime OccurredAt);
