using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Validators;

/// <summary>
/// Contains unit tests for the <see cref="CreateSaleValidator"/> class.
/// </summary>
public class CreateSaleValidatorTests
{
    private readonly CreateSaleValidator _validator = new();

    [Fact(DisplayName = "Given a valid command When validated Then passes")]
    public void Validate_ValidCommand_Passes()
    {
        var command = SaleHandlerTestData.GenerateValidCreateCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Given an empty sale number When validated Then fails")]
    public void Validate_EmptySaleNumber_Fails()
    {
        var command = SaleHandlerTestData.GenerateValidCreateCommand();
        command.SaleNumber = string.Empty;

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "Given no items When validated Then fails")]
    public void Validate_NoItems_Fails()
    {
        var command = SaleHandlerTestData.GenerateValidCreateCommand();
        command.Items = [];

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "Given an item quantity above 20 When validated Then fails")]
    public void Validate_ItemQuantityAboveLimit_Fails()
    {
        var command = SaleHandlerTestData.GenerateValidCreateCommand();
        command.Items[0].Quantity = 21;

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
