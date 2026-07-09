using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.ValueObjects;

/// <summary>
/// Contains unit tests for the External Identities value objects
/// (CustomerReference, BranchReference, ProductReference).
/// All three share the same guard clauses and value-equality semantics.
/// </summary>
public class ExternalIdentityReferenceTests
{
    /// <summary>
    /// Tests that an empty id is rejected when creating a customer reference.
    /// </summary>
    [Fact(DisplayName = "Empty id should not be allowed for a customer reference")]
    public void Given_EmptyId_When_CustomerReferenceCreated_Then_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new CustomerReference(Guid.Empty, "John Doe"));
    }

    /// <summary>
    /// Tests that a blank name is rejected when creating a branch reference.
    /// </summary>
    [Fact(DisplayName = "Blank name should not be allowed for a branch reference")]
    public void Given_BlankName_When_BranchReferenceCreated_Then_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new BranchReference(Guid.NewGuid(), "   "));
    }

    /// <summary>
    /// Tests that a blank name is rejected when creating a product reference.
    /// </summary>
    [Fact(DisplayName = "Blank name should not be allowed for a product reference")]
    public void Given_BlankName_When_ProductReferenceCreated_Then_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new ProductReference(Guid.NewGuid(), ""));
    }

    /// <summary>
    /// Tests that two product references with the same id and name are equal by value.
    /// </summary>
    [Fact(DisplayName = "References with the same id and name should be equal by value")]
    public void Given_SameIdAndName_When_Compared_Then_ReferencesAreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var first = new ProductReference(id, "Skol 350ml");
        var second = new ProductReference(id, "Skol 350ml");

        // Act & Assert
        Assert.Equal(first, second);
    }
}
