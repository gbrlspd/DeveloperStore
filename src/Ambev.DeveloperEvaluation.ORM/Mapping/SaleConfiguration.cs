using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.SaleNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.SaleNumber).IsUnique();

        builder.Property(s => s.SaleDate).IsRequired();
        builder.Property(s => s.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.IsCancelled).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt);

        // External Identities: Customer and Branch are owned by other domains,
        // so only their id and denormalized name are persisted alongside the sale.
        builder.OwnsOne(s => s.Customer, customer =>
        {
            customer.Property(c => c.Id).HasColumnName("CustomerId").IsRequired();
            customer.Property(c => c.Name).HasColumnName("CustomerName").IsRequired().HasMaxLength(200);
        });

        builder.OwnsOne(s => s.Branch, branch =>
        {
            branch.Property(b => b.Id).HasColumnName("BranchId").IsRequired();
            branch.Property(b => b.Name).HasColumnName("BranchName").IsRequired().HasMaxLength(200);
        });

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey("SaleId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
