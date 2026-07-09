using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the SaleItem entity class.
/// Tests cover the quantity-based discount policy and its boundaries.
/// </summary>
public class SaleItemTests
{
    /// <summary>
    /// Tests that the discount percentage matches the expected tier for each quantity boundary.
    /// </summary>
    [Theory(DisplayName = "Discount percentage should match the quantity tier")]
    [InlineData(1, 0.00)]
    [InlineData(3, 0.00)]
    [InlineData(4, 0.10)]
    [InlineData(9, 0.10)]
    [InlineData(10, 0.20)]
    [InlineData(20, 0.20)]
    public void Given_Quantity_When_ItemCreated_Then_DiscountPercentageMatchesTier(int quantity, decimal expectedDiscountPercentage)
    {
        // Arrange & Act
        var item = new SaleItem(SaleTestData.GenerateProduct(), quantity, 10m);

        // Assert
        Assert.Equal(expectedDiscountPercentage, item.DiscountPercentage);
    }

    /// <summary>
    /// Tests that selling more than 20 identical items is rejected.
    /// </summary>
    [Fact(DisplayName = "Selling more than 20 identical items should not be allowed")]
    public void Given_QuantityAboveLimit_When_ItemCreated_Then_ThrowsDomainException()
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new SaleItem(SaleTestData.GenerateProduct(), 21, 10m));
    }

    /// <summary>
    /// Tests that a zero or negative quantity is rejected.
    /// </summary>
    [Theory(DisplayName = "Zero or negative quantity should not be allowed")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Given_NonPositiveQuantity_When_ItemCreated_Then_ThrowsDomainException(int quantity)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new SaleItem(SaleTestData.GenerateProduct(), quantity, 10m));
    }

    /// <summary>
    /// Tests that a zero or negative unit price is rejected.
    /// </summary>
    [Fact(DisplayName = "Zero or negative unit price should not be allowed")]
    public void Given_NonPositiveUnitPrice_When_ItemCreated_Then_ThrowsDomainException()
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new SaleItem(SaleTestData.GenerateProduct(), 5, 0m));
    }

    /// <summary>
    /// Tests that the discount and total amount are computed correctly for a 20%-tier item.
    /// </summary>
    [Fact(DisplayName = "Discount and total amount should be computed correctly")]
    public void Given_ValidItem_When_Created_Then_DiscountAndTotalAreCorrect()
    {
        // Arrange & Act
        // 10 units * 100 = 1000; 20% discount = 200; total = 800
        var item = new SaleItem(SaleTestData.GenerateProduct(), 10, 100m);

        // Assert
        Assert.Equal(200m, item.Discount);
        Assert.Equal(800m, item.TotalAmount);
    }

    /// <summary>
    /// Tests that an item below the discount threshold has no discount applied.
    /// </summary>
    [Fact(DisplayName = "Item below discount threshold should have no discount")]
    public void Given_QuantityBelowThreshold_When_Created_Then_NoDiscountIsApplied()
    {
        // Arrange & Act
        var item = new SaleItem(SaleTestData.GenerateProduct(), 2, 50m);

        // Assert
        Assert.Equal(0m, item.Discount);
        Assert.Equal(100m, item.TotalAmount);
    }

    /// <summary>
    /// Tests that cancelling an item marks it as cancelled.
    /// </summary>
    [Fact(DisplayName = "Cancelling an item should mark it as cancelled")]
    public void Given_ActiveItem_When_Cancelled_Then_IsCancelledIsTrue()
    {
        // Arrange
        var item = new SaleItem(SaleTestData.GenerateProduct(), 5, 10m);

        // Act
        item.Cancel();

        // Assert
        Assert.True(item.IsCancelled);
    }

    /// <summary>
    /// Tests that cancelling an already cancelled item throws.
    /// </summary>
    [Fact(DisplayName = "Cancelling an already cancelled item should throw")]
    public void Given_CancelledItem_When_CancelledAgain_Then_ThrowsDomainException()
    {
        // Arrange
        var item = new SaleItem(SaleTestData.GenerateProduct(), 5, 10m);
        item.Cancel();

        // Act & Assert
        Assert.Throws<DomainException>(() => item.Cancel());
    }
}
