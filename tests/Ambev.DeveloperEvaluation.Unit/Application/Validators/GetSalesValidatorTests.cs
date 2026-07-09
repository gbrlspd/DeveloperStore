using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Validators;

/// <summary>
/// Contains unit tests for the <see cref="GetSalesValidator"/> class.
/// </summary>
public class GetSalesValidatorTests
{
    private readonly GetSalesValidator _validator = new();

    [Theory(DisplayName = "Given a valid page and size When validated Then passes")]
    [InlineData(1, 10)]
    [InlineData(5, 100)]
    public void Validate_ValidPageAndSize_Passes(int page, int size)
    {
        var command = new GetSalesCommand { Page = page, Size = size };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory(DisplayName = "Given an invalid page or size When validated Then fails")]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Validate_InvalidPageOrSize_Fails(int page, int size)
    {
        var command = new GetSalesCommand { Page = page, Size = size };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
