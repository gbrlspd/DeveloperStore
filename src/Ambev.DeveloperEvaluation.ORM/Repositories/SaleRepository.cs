using System.Linq.Expressions;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Implementation of ISaleRepository using Entity Framework Core.
/// </summary>
public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    /// <summary>
    /// Initializes a new instance of SaleRepository.
    /// </summary>
    /// <param name="context">The database context.</param>
    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new sale, including its items, in the database.
    /// </summary>
    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    /// <summary>
    /// Retrieves a sale by its unique identifier, including its items.
    /// </summary>
    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves a paginated, ordered and filtered list of sales.
    /// </summary>
    public async Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? order = null,
        IDictionary<string, string>? filters = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales.Include(s => s.Items).AsQueryable();

        query = ApplyFilters(query, filters);
        query = ApplyOrder(query, order);

        var totalCount = await query.CountAsync(cancellationToken);

        var sales = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (sales, totalCount);
    }

    /// <summary>
    /// Persists changes made to an existing sale. The sale is expected to already be
    /// tracked by this context (retrieved via GetByIdAsync earlier in the same scope).
    ///
    /// SaleItem's Id is client-generated (assigned in its constructor), so when a new
    /// item reaches the change tracker only through navigation fixup -- e.g. after
    /// Sale.ClearItems()+AddItem() -- EF Core cannot tell it apart from a pre-existing
    /// one and defaults its state to Modified instead of Added, which would issue an
    /// UPDATE for a row that was never inserted. We resolve the ambiguity by checking
    /// which of the currently Modified items actually exist in the database: those that
    /// don't are new and get flipped to Added; genuinely modified existing items (e.g.
    /// an item cancelled in place) are left as Modified.
    /// </summary>
    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _context.ChangeTracker.DetectChanges();

        var modifiedItems = _context.ChangeTracker.Entries<SaleItem>()
            .Where(entry => entry.State == EntityState.Modified)
            .ToList();

        if (modifiedItems.Count > 0)
        {
            var modifiedItemIds = modifiedItems.Select(entry => entry.Entity.Id).ToList();
            var existingItemIds = await _context.Set<SaleItem>()
                .AsNoTracking()
                .Where(item => modifiedItemIds.Contains(item.Id))
                .Select(item => item.Id)
                .ToListAsync(cancellationToken);

            var existingItemIdSet = existingItemIds.ToHashSet();

            foreach (var entry in modifiedItems)
            {
                if (!existingItemIdSet.Contains(entry.Entity.Id))
                    entry.State = EntityState.Added;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    /// <summary>
    /// Deletes a sale from the database.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (sale is null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Applies field=value filters following the general API filtering conventions:
    /// string fields support a leading/trailing '*' for partial (case-insensitive) matches,
    /// and _min/_max prefixes support range filters over dates and amounts.
    /// </summary>
    private static IQueryable<Sale> ApplyFilters(IQueryable<Sale> query, IDictionary<string, string>? filters)
    {
        if (filters is null || filters.Count == 0)
            return query;

        foreach (var (rawField, value) in filters)
        {
            query = rawField.ToLowerInvariant() switch
            {
                "salenumber" => query.Where(s => EF.Functions.ILike(s.SaleNumber, ToLikePattern(value))),
                "customer" or "customername" => query.Where(s => EF.Functions.ILike(s.Customer.Name, ToLikePattern(value))),
                "branch" or "branchname" => query.Where(s => EF.Functions.ILike(s.Branch.Name, ToLikePattern(value))),
                "iscancelled" => query.Where(s => s.IsCancelled == bool.Parse(value)),
                "_minsaledate" => query.Where(s => s.SaleDate >= DateTime.Parse(value)),
                "_maxsaledate" => query.Where(s => s.SaleDate <= DateTime.Parse(value)),
                "_mintotalamount" => query.Where(s => s.TotalAmount >= decimal.Parse(value)),
                "_maxtotalamount" => query.Where(s => s.TotalAmount <= decimal.Parse(value)),
                _ => query
            };
        }

        return query;
    }

    /// <summary>
    /// Translates a general-API-style wildcard value (e.g. "Fjallraven*", "*clothing")
    /// into a SQL LIKE pattern. A value without '*' is matched exactly (case-insensitively).
    /// </summary>
    private static string ToLikePattern(string value)
    {
        if (value.Length > 1 && value.StartsWith('*') && value.EndsWith('*'))
            return $"%{value.Trim('*')}%";

        if (value.EndsWith('*'))
            return $"{value[..^1]}%";

        if (value.StartsWith('*'))
            return $"%{value[1..]}";

        return value;
    }

    /// <summary>
    /// Applies a comma-separated ordering expression (e.g. "saleDate desc, saleNumber")
    /// following the general API ordering conventions. Defaults to SaleDate descending.
    /// </summary>
    private static IQueryable<Sale> ApplyOrder(IQueryable<Sale> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return query.OrderByDescending(s => s.SaleDate);

        IOrderedQueryable<Sale>? ordered = null;

        foreach (var clause in order.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = clause.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = parts[0].ToLowerInvariant();
            var descending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            ordered = field switch
            {
                "salenumber" => ApplyOrderClause(ordered, query, s => s.SaleNumber, descending),
                "saledate" => ApplyOrderClause(ordered, query, s => s.SaleDate, descending),
                "totalamount" => ApplyOrderClause(ordered, query, s => s.TotalAmount, descending),
                _ => ordered
            };
        }

        return ordered ?? query.OrderByDescending(s => s.SaleDate);
    }

    private static IOrderedQueryable<Sale> ApplyOrderClause<TKey>(
        IOrderedQueryable<Sale>? ordered,
        IQueryable<Sale> query,
        Expression<Func<Sale, TKey>> keySelector,
        bool descending)
    {
        if (ordered is null)
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

        return descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
    }
}
