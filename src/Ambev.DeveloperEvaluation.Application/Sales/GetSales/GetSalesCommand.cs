using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

/// <summary>
/// Command for retrieving a paginated, ordered and filtered list of sales.
/// </summary>
public class GetSalesCommand : IRequest<GetSalesResult>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;

    /// <summary>
    /// Comma-separated ordering expression, e.g. "saleDate desc, saleNumber".
    /// </summary>
    public string? Order { get; set; }

    /// <summary>
    /// Field=value filters following the general API filtering conventions.
    /// </summary>
    public IDictionary<string, string>? Filters { get; set; }
}
