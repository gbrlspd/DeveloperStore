namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Raised when a single item within a sale is cancelled.
/// </summary>
public sealed record ItemCancelledEvent(Guid SaleId, string SaleNumber, Guid ItemId, DateTime OccurredAt);
