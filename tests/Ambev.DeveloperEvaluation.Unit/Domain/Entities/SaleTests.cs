using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the Sale aggregate root.
/// Tests cover total calculation, cancellation and validation scenarios.
/// </summary>
public class SaleTests
{
    /// <summary>
    /// Tests that the sale total is the sum of every non-cancelled item's total.
    /// </summary>
    [Fact(DisplayName = "Sale total should be the sum of non-cancelled items")]
    public void Given_SaleWithItems_When_ItemCancelled_Then_TotalExcludesCancelledItem()
    {
        // Arrange
        var sale = SaleTestData.GenerateEmptySale();
        var firstItem = sale.AddItem(SaleTestData.GenerateProduct(), 5, 10m);  // 50 - 10% = 45
        var secondItem = sale.AddItem(SaleTestData.GenerateProduct(), 10, 20m); // 200 - 20% = 160

        Assert.Equal(45m + 160m, sale.TotalAmount);

        // Act
        sale.CancelItem(secondItem.Id);

        // Assert
        Assert.Equal(45m, sale.TotalAmount);
        Assert.True(secondItem.IsCancelled);
        Assert.False(firstItem.IsCancelled);
    }

    /// <summary>
    /// Tests that cancelling the sale marks it as cancelled.
    /// </summary>
    [Fact(DisplayName = "Cancelling a sale should mark it as cancelled")]
    public void Given_ActiveSale_When_Cancelled_Then_IsCancelledIsTrue()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        sale.Cancel();

        // Assert
        Assert.True(sale.IsCancelled);
    }

    /// <summary>
    /// Tests that cancelling an already cancelled sale throws.
    /// </summary>
    [Fact(DisplayName = "Cancelling an already cancelled sale should throw")]
    public void Given_CancelledSale_When_CancelledAgain_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.Cancel();

        // Act & Assert
        Assert.Throws<DomainException>(() => sale.Cancel());
    }

    /// <summary>
    /// Tests that adding an item to a cancelled sale throws.
    /// </summary>
    [Fact(DisplayName = "Adding an item to a cancelled sale should throw")]
    public void Given_CancelledSale_When_ItemAdded_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.Cancel();

        // Act & Assert
        Assert.Throws<DomainException>(() => sale.AddItem(SaleTestData.GenerateProduct(), 1, 10m));
    }

    /// <summary>
    /// Tests that cancelling an unknown item throws.
    /// </summary>
    [Fact(DisplayName = "Cancelling an unknown item should throw")]
    public void Given_UnknownItemId_When_Cancelled_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act & Assert
        Assert.Throws<DomainException>(() => sale.CancelItem(Guid.NewGuid()));
    }

    /// <summary>
    /// Tests that cancelling an already cancelled item throws.
    /// </summary>
    [Fact(DisplayName = "Cancelling an already cancelled item should throw")]
    public void Given_CancelledItem_When_CancelledAgain_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item = sale.Items.First();
        sale.CancelItem(item.Id);

        // Act & Assert
        Assert.Throws<DomainException>(() => sale.CancelItem(item.Id));
    }

    /// <summary>
    /// Tests that clearing items resets the sale total to zero.
    /// </summary>
    [Fact(DisplayName = "Clearing items should reset the sale total")]
    public void Given_SaleWithItems_When_ItemsCleared_Then_TotalIsZero()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        sale.ClearItems();

        // Assert
        Assert.Empty(sale.Items);
        Assert.Equal(0m, sale.TotalAmount);
    }

    /// <summary>
    /// Tests that updating the details of a cancelled sale throws.
    /// </summary>
    [Fact(DisplayName = "Updating details of a cancelled sale should throw")]
    public void Given_CancelledSale_When_DetailsUpdated_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.Cancel();

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            sale.UpdateDetails(DateTime.UtcNow, SaleTestData.GenerateCustomer(), SaleTestData.GenerateBranch()));
    }

    /// <summary>
    /// Tests that validation passes for a valid sale with at least one item.
    /// </summary>
    [Fact(DisplayName = "Validation should pass for a valid sale")]
    public void Given_ValidSale_When_Validated_Then_ShouldReturnValid()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        var result = sale.Validate();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Tests that validation fails for a sale without any items.
    /// </summary>
    [Fact(DisplayName = "Validation should fail for a sale without items")]
    public void Given_SaleWithoutItems_When_Validated_Then_ShouldReturnInvalid()
    {
        // Arrange
        var sale = SaleTestData.GenerateEmptySale();

        // Act
        var result = sale.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }
}
