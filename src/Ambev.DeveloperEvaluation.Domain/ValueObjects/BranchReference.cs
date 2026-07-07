namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// Represents a denormalized reference to a Branch owned by another domain.
/// Follows the External Identities pattern: only the identifier and the
/// descriptive name are kept here, avoiding a hard dependency on the Branch domain.
/// </summary>
public sealed record BranchReference
{
    /// <summary>
    /// Gets the unique identifier of the branch in its own domain.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the denormalized branch name, captured at the time of the sale.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Parameterless constructor required for ORM materialization.
    /// </summary>
    public BranchReference()
    {
    }

    /// <summary>
    /// Initializes a new branch reference, validating the external identity.
    /// </summary>
    /// <param name="id">The branch's unique identifier.</param>
    /// <param name="name">The branch's denormalized name.</param>
    public BranchReference(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new DomainException("Branch id must be provided.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Branch name must be provided.");

        Id = id;
        Name = name;
    }
}
