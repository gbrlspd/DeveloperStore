using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales;

/// <summary>
/// Validates a <see cref="SaleItemInput"/> submitted on Create/Update sale commands.
/// Mirrors the Domain's SaleItemValidator so invalid input fails fast, before
/// the command even reaches the aggregate.
/// </summary>
public class SaleItemInputValidator : AbstractValidator<SaleItemInput>
{
    public SaleItemInputValidator()
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
