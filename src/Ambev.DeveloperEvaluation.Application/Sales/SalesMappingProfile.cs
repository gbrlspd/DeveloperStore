using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales;

/// <summary>
/// Maps Sale/SaleItem entities to their result DTOs. Shared by CreateSale,
/// GetSale, GetSales and UpdateSale, since they all return the same shape.
/// AutoMapper's flattening convention maps the owned value objects
/// (Customer.Id/Name, Branch.Id/Name, Product.Id/Name) onto the
/// corresponding CustomerId/CustomerName, BranchId/BranchName and
/// ProductId/ProductName members automatically.
/// </summary>
public class SalesMappingProfile : Profile
{
    public SalesMappingProfile()
    {
        CreateMap<Sale, SaleResult>();
        CreateMap<SaleItem, SaleItemResult>();
    }
}
