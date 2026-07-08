using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Validator for CreateSaleCommand.
/// </summary>
public class CreateSaleValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidator()
    {
        RuleFor(command => command.SaleNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(command => command.SaleDate)
            .NotEqual(default(DateTime));

        RuleFor(command => command.CustomerId).NotEmpty();
        RuleFor(command => command.CustomerName).NotEmpty();
        RuleFor(command => command.BranchId).NotEmpty();
        RuleFor(command => command.BranchName).NotEmpty();

        RuleFor(command => command.Items)
            .NotEmpty().WithMessage("Sale must contain at least one item.");

        RuleForEach(command => command.Items).SetValidator(new SaleItemInputValidator());
    }
}
