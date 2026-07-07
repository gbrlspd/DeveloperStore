using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository interface for Sale aggregate operations.
/// </summary>
public interface ISaleRepository
{
    /// <summary>
    /// Creates a new sale in the repository.
    /// </summary>
    /// <param name="sale">The sale to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created sale.</returns>
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a sale by its unique identifier, including its items.
    /// </summary>
    /// <param name="id">The unique identifier of the sale.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sale if found, null otherwise.</returns>
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated, ordered and filtered list of sales.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="size">The number of items per page.</param>
    /// <param name="order">
    /// An optional comma-separated ordering expression, e.g. "saleDate desc, saleNumber".
    /// </param>
    /// <param name="filters">
    /// An optional set of field-value filters, following the general API filtering conventions.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching sales for the requested page and the total matching count.</returns>
    Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? order = null,
        IDictionary<string, string>? filters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes made to an existing sale.
    /// </summary>
    /// <param name="sale">The sale to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated sale.</returns>
    Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a sale from the repository.
    /// </summary>
    /// <param name="id">The unique identifier of the sale to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the sale was deleted, false if not found.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
