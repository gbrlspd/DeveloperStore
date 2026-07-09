using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// Validator for SaleItemRequest, shared by CreateSaleRequestValidator and
/// UpdateSaleRequestValidator.
/// </summary>
public class SaleItemRequestValidator : AbstractValidator<SaleItemRequest>
{
    public SaleItemRequestValidator()
    {
        RuleFor(item => item.ProductId).NotEmpty();
        RuleFor(item => item.ProductName).NotEmpty();

        RuleFor(item => item.Quantity)
            .GreaterThan(0).WithMessage("Item quantity must be greater than zero.")
            .LessThanOrEqualTo(20).WithMessage("It's not possible to sell above 20 identical items.");

        RuleFor(item => item.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than zero.");
    }
}
