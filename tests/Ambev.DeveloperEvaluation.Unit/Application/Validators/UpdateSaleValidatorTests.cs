using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Validators;

/// <summary>
/// Contains unit tests for the <see cref="UpdateSaleValidator"/> class.
/// </summary>
public class UpdateSaleValidatorTests
{
    private readonly UpdateSaleValidator _validator = new();

    [Fact(DisplayName = "Given a valid command When validated Then passes")]
    public void Validate_ValidCommand_Passes()
    {
        var command = SaleHandlerTestData.GenerateValidUpdateCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Given an empty id When validated Then fails")]
    public void Validate_EmptyId_Fails()
    {
        var command = SaleHandlerTestData.GenerateValidUpdateCommand();
        command.Id = Guid.Empty;

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "Given no items When validated Then fails")]
    public void Validate_NoItems_Fails()
    {
        var command = SaleHandlerTestData.GenerateValidUpdateCommand();
        command.Items = [];

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
