using Ambev.DeveloperEvaluation.WebApi.Features.Sales;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// Validator for UpdateSaleRequest.
/// </summary>
public class UpdateSaleRequestValidator : AbstractValidator<UpdateSaleRequest>
{
    public UpdateSaleRequestValidator()
    {
        RuleFor(request => request.SaleDate).NotEqual(default(DateTime));
        RuleFor(request => request.CustomerId).NotEmpty();
        RuleFor(request => request.CustomerName).NotEmpty();
        RuleFor(request => request.BranchId).NotEmpty();
        RuleFor(request => request.BranchName).NotEmpty();

        RuleFor(request => request.Items)
            .NotEmpty().WithMessage("Sale must contain at least one item.");

        RuleForEach(request => request.Items).SetValidator(new SaleItemRequestValidator());
    }
}
