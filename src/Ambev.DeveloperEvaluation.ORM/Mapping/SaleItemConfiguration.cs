using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(i => i.DiscountPercentage).HasColumnType("decimal(5,4)");
        builder.Property(i => i.Discount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.IsCancelled).IsRequired();

        // External Identity: Product is owned by another domain, so only its
        // id and denormalized name are persisted alongside the sale item.
        builder.OwnsOne(i => i.Product, product =>
        {
            product.Property(p => p.Id).HasColumnName("ProductId").IsRequired();
            product.Property(p => p.Name).HasColumnName("ProductName").IsRequired().HasMaxLength(200);
        });
    }
}
