using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// Maps between the Sales WebApi request/response models and the
/// Application layer's commands/results.
/// </summary>
public class SalesProfile : Profile
{
    public SalesProfile()
    {
        CreateMap<SaleItemRequest, SaleItemInput>();

        CreateMap<CreateSaleRequest, CreateSaleCommand>();

        CreateMap<UpdateSaleRequest, UpdateSaleCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<SaleResult, SaleResponse>();
        CreateMap<SaleItemResult, SaleItemResponse>();

        CreateMap<Guid, GetSaleCommand>().ConstructUsing(id => new GetSaleCommand(id));
        CreateMap<Guid, DeleteSaleCommand>().ConstructUsing(id => new DeleteSaleCommand(id));
        CreateMap<Guid, CancelSaleCommand>().ConstructUsing(id => new CancelSaleCommand(id));
    }
}
