using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

/// <summary>
/// Provides methods for generating Sales Application-layer test data
/// (commands and existing domain entities) using the Bogus library.
/// </summary>
public static class SaleHandlerTestData
{
    private static readonly Faker Faker = new();

    public static string GenerateSaleNumber() => $"SALE-{Faker.Random.Number(100000, 999999)}";

    public static decimal GenerateUnitPrice() => Faker.Random.Decimal(1, 500);

    public static SaleItemInput GenerateItemInput(int quantity = 5)
    {
        return new SaleItemInput
        {
            ProductId = Guid.NewGuid(),
            ProductName = Faker.Commerce.ProductName(),
            Quantity = quantity,
            UnitPrice = GenerateUnitPrice()
        };
    }

    /// <summary>
    /// Generates a valid CreateSaleCommand with a single item.
    /// </summary>
    public static CreateSaleCommand GenerateValidCreateCommand()
    {
        return new CreateSaleCommand
        {
            SaleNumber = GenerateSaleNumber(),
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = Faker.Person.FullName,
            BranchId = Guid.NewGuid(),
            BranchName = Faker.Company.CompanyName(),
            Items = [GenerateItemInput()]
        };
    }

    /// <summary>
    /// Generates a valid UpdateSaleCommand with a single item.
    /// </summary>
    public static UpdateSaleCommand GenerateValidUpdateCommand(Guid? id = null)
    {
        return new UpdateSaleCommand
        {
            Id = id ?? Guid.NewGuid(),
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = Faker.Person.FullName,
            BranchId = Guid.NewGuid(),
            BranchName = Faker.Company.CompanyName(),
            Items = [GenerateItemInput()]
        };
    }

    /// <summary>
    /// Generates an existing Sale aggregate (as if loaded from the repository),
    /// with a single item, optionally with a specific Id.
    /// </summary>
    public static Sale GenerateExistingSale(Guid? id = null)
    {
        var sale = new Sale(
            GenerateSaleNumber(),
            DateTime.UtcNow,
            new CustomerReference(Guid.NewGuid(), Faker.Person.FullName),
            new BranchReference(Guid.NewGuid(), Faker.Company.CompanyName()));

        sale.AddItem(new ProductReference(Guid.NewGuid(), Faker.Commerce.ProductName()), 5, GenerateUnitPrice());

        if (id.HasValue)
            sale.Id = id.Value;

        return sale;
    }
}
