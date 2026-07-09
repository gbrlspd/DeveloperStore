namespace Ambev.DeveloperEvaluation.Application.Sales;

/// <summary>
/// Represents a single item submitted when creating or updating a sale.
/// </summary>
public class SaleItemInput
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
