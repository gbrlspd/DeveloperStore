namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

/// <summary>
/// Represents a page of sales along with the total matching count.
/// </summary>
public class GetSalesResult
{
    public IEnumerable<SaleResult> Sales { get; set; } = [];
    public int TotalCount { get; set; }
}
