using Ambev.DeveloperEvaluation.WebApi.Features.Sales;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>
/// Validator for CreateSaleRequest.
/// </summary>
public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(request => request.SaleNumber).NotEmpty().MaximumLength(50);
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
