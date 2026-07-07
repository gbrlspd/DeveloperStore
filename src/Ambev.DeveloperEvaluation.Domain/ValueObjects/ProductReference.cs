namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// Represents a denormalized reference to a Product owned by another domain.
/// Follows the External Identities pattern: only the identifier and the
/// descriptive name are kept here, avoiding a hard dependency on the Product domain.
/// </summary>
public sealed record ProductReference
{
    /// <summary>
    /// Gets the unique identifier of the product in its own domain.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the denormalized product name, captured at the time of the sale.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Parameterless constructor required for ORM materialization.
    /// </summary>
    public ProductReference()
    {
    }

    /// <summary>
    /// Initializes a new product reference, validating the external identity.
    /// </summary>
    /// <param name="id">The product's unique identifier.</param>
    /// <param name="name">The product's denormalized name.</param>
    public ProductReference(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new DomainException("Product id must be provided.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name must be provided.");

        Id = id;
        Name = name;
    }
}
