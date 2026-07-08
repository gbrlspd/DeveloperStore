namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// API response model representing a single item within a <see cref="SaleResponse"/>.
/// </summary>
public class SaleItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
}
