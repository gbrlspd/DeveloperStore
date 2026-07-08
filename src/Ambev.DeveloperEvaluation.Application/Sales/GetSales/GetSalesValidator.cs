using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

/// <summary>
/// Validator for GetSalesCommand.
/// </summary>
public class GetSalesValidator : AbstractValidator<GetSalesCommand>
{
    public GetSalesValidator()
    {
        RuleFor(command => command.Page).GreaterThanOrEqualTo(1);
        RuleFor(command => command.Size).InclusiveBetween(1, 100);
    }
}
