using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Validator for UpdateSaleCommand.
/// </summary>
public class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.SaleDate).NotEqual(default(DateTime));
        RuleFor(command => command.CustomerId).NotEmpty();
        RuleFor(command => command.CustomerName).NotEmpty();
        RuleFor(command => command.BranchId).NotEmpty();
        RuleFor(command => command.BranchName).NotEmpty();

        RuleFor(command => command.Items)
            .NotEmpty().WithMessage("Sale must contain at least one item.");

        RuleForEach(command => command.Items).SetValidator(new SaleItemInputValidator());
    }
}
