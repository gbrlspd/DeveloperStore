namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// API request model for a single item submitted when creating or updating a sale.
/// </summary>
public class SaleItemRequest
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
