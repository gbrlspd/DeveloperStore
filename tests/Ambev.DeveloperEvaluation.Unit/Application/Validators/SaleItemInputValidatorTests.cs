using Ambev.DeveloperEvaluation.Application.Sales;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Validators;

/// <summary>
/// Contains unit tests for the <see cref="SaleItemInputValidator"/> class.
/// </summary>
public class SaleItemInputValidatorTests
{
    private readonly SaleItemInputValidator _validator = new();

    private static SaleItemInput ValidItem() => new()
    {
        ProductId = Guid.NewGuid(),
        ProductName = "Skol 350ml",
        Quantity = 5,
        UnitPrice = 10m
    };

    [Fact(DisplayName = "Given a valid item When validated Then passes")]
    public void Validate_ValidItem_Passes()
    {
        var result = _validator.Validate(ValidItem());

        result.IsValid.Should().BeTrue();
    }

    [Theory(DisplayName = "Given a quantity outside 1-20 When validated Then fails")]
    [InlineData(0)]
    [InlineData(21)]
    public void Validate_QuantityOutOfRange_Fails(int quantity)
    {
        var item = ValidItem();
        item.Quantity = quantity;

        var result = _validator.Validate(item);

        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "Given a zero unit price When validated Then fails")]
    public void Validate_ZeroUnitPrice_Fails()
    {
        var item = ValidItem();
        item.UnitPrice = 0m;

        var result = _validator.Validate(item);

        result.IsValid.Should().BeFalse();
    }
}
