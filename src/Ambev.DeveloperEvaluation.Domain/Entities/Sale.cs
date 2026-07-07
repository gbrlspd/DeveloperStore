using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a sale record: the aggregate root for the Sales domain.
/// Customer, Branch and each item's Product are referenced using the
/// External Identities pattern with denormalized descriptions, since those
/// entities are owned by other domains.
/// </summary>
public class Sale : BaseEntity
{
    private readonly List<SaleItem> _items = [];

    /// <summary>
    /// Gets the human-readable, unique sale number.
    /// </summary>
    public string SaleNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the date and time (UTC) when the sale was made.
    /// </summary>
    public DateTime SaleDate { get; private set; }

    /// <summary>
    /// Gets the denormalized reference to the customer who made the purchase.
    /// </summary>
    public CustomerReference Customer { get; private set; } = null!;

    /// <summary>
    /// Gets the denormalized reference to the branch where the sale was made.
    /// </summary>
    public BranchReference Branch { get; private set; } = null!;

    /// <summary>
    /// Gets the read-only collection of items included in this sale.
    /// </summary>
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Gets the total sale amount: the sum of the total amount of every non-cancelled item.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the entire sale has been cancelled.
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Gets the date and time when the sale was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the date and time of the last update to the sale.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Parameterless constructor required for ORM materialization.
    /// </summary>
    protected Sale()
    {
    }

    /// <summary>
    /// Creates a new sale for the given customer and branch.
    /// </summary>
    /// <param name="saleNumber">The unique, human-readable sale number.</param>
    /// <param name="saleDate">The date and time (UTC) the sale was made.</param>
    /// <param name="customer">The denormalized reference to the customer.</param>
    /// <param name="branch">The denormalized reference to the branch.</param>
    public Sale(string saleNumber, DateTime saleDate, CustomerReference customer, BranchReference branch)
    {
        if (string.IsNullOrWhiteSpace(saleNumber))
            throw new DomainException("Sale number must be provided.");

        Id = Guid.NewGuid();
        SaleNumber = saleNumber;
        SaleDate = saleDate;
        Customer = customer ?? throw new DomainException("Customer reference must be provided.");
        Branch = branch ?? throw new DomainException("Branch reference must be provided.");
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a new item to the sale, applying the quantity-based discount policy.
    /// </summary>
    /// <param name="product">The denormalized reference to the product.</param>
    /// <param name="quantity">The quantity of identical items sold (1-20).</param>
    /// <param name="unitPrice">The unit price of the product.</param>
    /// <returns>The newly added <see cref="SaleItem"/>.</returns>
    /// <exception cref="DomainException">Thrown when the sale is already cancelled.</exception>
    public SaleItem AddItem(ProductReference product, int quantity, decimal unitPrice)
    {
        if (IsCancelled)
            throw new DomainException("Cannot add items to a cancelled sale.");

        var item = new SaleItem(product, quantity, unitPrice);
        _items.Add(item);
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;

        return item;
    }

    /// <summary>
    /// Removes every item currently in the sale, so a new set can be added.
    /// Used when updating a sale's items.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the sale is already cancelled.</exception>
    public void ClearItems()
    {
        if (IsCancelled)
            throw new DomainException("Cannot update items on a cancelled sale.");

        _items.Clear();
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the sale's descriptive details (date, customer and branch).
    /// </summary>
    /// <exception cref="DomainException">Thrown when the sale is already cancelled.</exception>
    public void UpdateDetails(DateTime saleDate, CustomerReference customer, BranchReference branch)
    {
        if (IsCancelled)
            throw new DomainException("Cannot update a cancelled sale.");

        SaleDate = saleDate;
        Customer = customer ?? throw new DomainException("Customer reference must be provided.");
        Branch = branch ?? throw new DomainException("Branch reference must be provided.");
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels a specific item within the sale and recalculates the sale total.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item to cancel.</param>
    /// <exception cref="DomainException">Thrown when the item cannot be found.</exception>
    public void CancelItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Item {itemId} was not found in sale {SaleNumber}.");

        item.Cancel();
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the entire sale.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the sale is already cancelled.</exception>
    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("Sale is already cancelled.");

        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Recalculates <see cref="TotalAmount"/> as the sum of every non-cancelled item's total.
    /// </summary>
    private void RecalculateTotal()
    {
        TotalAmount = _items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount);
    }

    /// <summary>
    /// Validates the sale using the <see cref="SaleValidator"/> rules.
    /// </summary>
    /// <returns>A <see cref="ValidationResultDetail"/> describing the outcome.</returns>
    public ValidationResultDetail Validate()
    {
        var validator = new SaleValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}
