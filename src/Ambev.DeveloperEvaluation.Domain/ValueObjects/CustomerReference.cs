namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// Represents a denormalized reference to a Customer owned by another domain.
/// Follows the External Identities pattern: only the identifier and the
/// descriptive name are kept here, avoiding a hard dependency on the Customer domain.
/// </summary>
public sealed record CustomerReference
{
    /// <summary>
    /// Gets the unique identifier of the customer in its own domain.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the denormalized customer name, captured at the time of the sale.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Parameterless constructor required for ORM materialization.
    /// </summary>
    public CustomerReference()
    {
    }

    /// <summary>
    /// Initializes a new customer reference, validating the external identity.
    /// </summary>
    /// <param name="id">The customer's unique identifier.</param>
    /// <param name="name">The customer's denormalized name.</param>
    public CustomerReference(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new DomainException("Customer id must be provided.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Customer name must be provided.");

        Id = id;
        Name = name;
    }
}
