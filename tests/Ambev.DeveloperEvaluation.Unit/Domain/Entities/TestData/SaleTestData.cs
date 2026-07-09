using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

/// <summary>
/// Provides methods for generating Sales test data using the Bogus library.
/// Centralizes test data generation to ensure consistency across test cases.
/// </summary>
public static class SaleTestData
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Generates a unique, human-readable sale number.
    /// </summary>
    public static string GenerateSaleNumber()
    {
        return $"SALE-{Faker.Random.Number(100000, 999999)}";
    }

    /// <summary>
    /// Generates a valid unit price.
    /// </summary>
    public static decimal GenerateUnitPrice()
    {
        return Faker.Random.Decimal(1, 500);
    }

    /// <summary>
    /// Generates a denormalized customer reference.
    /// </summary>
    public static CustomerReference GenerateCustomer()
    {
        return new CustomerReference(Guid.NewGuid(), Faker.Person.FullName);
    }

    /// <summary>
    /// Generates a denormalized branch reference.
    /// </summary>
    public static BranchReference GenerateBranch()
    {
        return new BranchReference(Guid.NewGuid(), Faker.Company.CompanyName());
    }

    /// <summary>
    /// Generates a denormalized product reference.
    /// </summary>
    public static ProductReference GenerateProduct()
    {
        return new ProductReference(Guid.NewGuid(), Faker.Commerce.ProductName());
    }

    /// <summary>
    /// Generates a valid sale with no items yet.
    /// </summary>
    public static Sale GenerateEmptySale()
    {
        return new Sale(GenerateSaleNumber(), DateTime.UtcNow, GenerateCustomer(), GenerateBranch());
    }

    /// <summary>
    /// Generates a valid sale containing a single item with the given quantity.
    /// </summary>
    public static Sale GenerateValidSale(int quantity = 5)
    {
        var sale = GenerateEmptySale();
        sale.AddItem(GenerateProduct(), quantity, GenerateUnitPrice());
        return sale;
    }
}
