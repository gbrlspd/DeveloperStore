namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Raised when an existing sale's details or items are modified.
/// </summary>
public sealed record SaleModifiedEvent(Guid SaleId, string SaleNumber, DateTime OccurredAt);
