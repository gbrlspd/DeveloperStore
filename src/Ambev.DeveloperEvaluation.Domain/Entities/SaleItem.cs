using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a single line item within a Sale: a product, the quantity sold,
/// the unit price and the resulting discount and total, following the
/// quantity-based discount policy defined for the DeveloperStore catalog.
/// </summary>
public class SaleItem : BaseEntity
{
    /// <summary>
    /// Gets the denormalized reference to the product being sold.
    /// </summary>
    public ProductReference Product { get; private set; } = null!;

    /// <summary>
    /// Gets the quantity of identical items sold. Limited to the 1-20 range.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Gets the unit price of the product at the time of the sale.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Gets the discount percentage applied based on the quantity tier.
    /// </summary>
    public decimal DiscountPercentage { get; private set; }

    /// <summary>
    /// Gets the discount amount, in currency, applied to this item.
    /// </summary>
    public decimal Discount { get; private set; }

    /// <summary>
    /// Gets the total amount for this item, after discount.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this item has been cancelled.
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Parameterless constructor required for ORM materialization.
    /// </summary>
    protected SaleItem()
    {
    }

    /// <summary>
    /// Creates a new sale item, applying the quantity-based discount policy.
    /// </summary>
    /// <param name="product">The denormalized product reference.</param>
    /// <param name="quantity">The quantity of identical items sold (1-20).</param>
    /// <param name="unitPrice">The unit price of the product.</param>
    /// <exception cref="DomainException">
    /// Thrown when the quantity or unit price violate the business rules.
    /// </exception>
    public SaleItem(ProductReference product, int quantity, decimal unitPrice)
    {
        if (product is null)
            throw new DomainException("A product reference must be provided.");

        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero.");

        Id = Guid.NewGuid();
        Product = product;
        Quantity = quantity;
        UnitPrice = unitPrice;

        ApplyDiscountPolicy();
    }

    /// <summary>
    /// Cancels this item.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the item is already cancelled.</exception>
    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("Item is already cancelled.");

        IsCancelled = true;
    }

    /// <summary>
    /// Applies the quantity-based discount policy:
    /// - More than 20 identical items is not allowed.
    /// - 10 to 20 identical items grants a 20% discount.
    /// - 4 to 9 identical items grants a 10% discount.
    /// - Fewer than 4 items is not eligible for any discount.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the quantity exceeds the allowed limit.</exception>
    private void ApplyDiscountPolicy()
    {
        if (Quantity <= 0)
            throw new DomainException("Item quantity must be greater than zero.");

        if (Quantity > 20)
            throw new DomainException("It's not possible to sell above 20 identical items.");

        DiscountPercentage = Quantity switch
        {
            >= 10 => 0.20m,
            >= 4 => 0.10m,
            _ => 0.00m
        };

        Discount = Math.Round(Quantity * UnitPrice * DiscountPercentage, 2, MidpointRounding.AwayFromZero);
        TotalAmount = (Quantity * UnitPrice) - Discount;
    }
}
