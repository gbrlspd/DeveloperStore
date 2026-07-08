using Ambev.DeveloperEvaluation.WebApi.Features.Sales;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// Represents a request to update an existing sale's details and items.
/// The sale's ID comes from the route, not the body.
/// </summary>
public class UpdateSaleRequest
{
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<SaleItemRequest> Items { get; set; } = [];
}
