using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

/// <summary>
/// Validates the invariants of a <see cref="Sale"/> aggregate.
/// </summary>
public class SaleValidator : AbstractValidator<Sale>
{
    public SaleValidator()
    {
        RuleFor(sale => sale.SaleNumber)
            .NotEmpty()
            .MaximumLength(50).WithMessage("Sale number cannot be longer than 50 characters.");

        RuleFor(sale => sale.SaleDate)
            .NotEqual(default(DateTime)).WithMessage("Sale date must be provided.");

        RuleFor(sale => sale.Customer)
            .NotNull().WithMessage("Sale must reference a customer.");

        RuleFor(sale => sale.Branch)
            .NotNull().WithMessage("Sale must reference a branch.");

        RuleFor(sale => sale.Items)
            .NotEmpty().WithMessage("Sale must contain at least one item.");

        RuleForEach(sale => sale.Items)
            .SetValidator(new SaleItemValidator());
    }
}
