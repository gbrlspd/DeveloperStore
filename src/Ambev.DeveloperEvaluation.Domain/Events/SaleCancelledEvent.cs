namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Raised when an entire sale is cancelled.
/// </summary>
public sealed record SaleCancelledEvent(Guid SaleId, string SaleNumber, DateTime OccurredAt);
