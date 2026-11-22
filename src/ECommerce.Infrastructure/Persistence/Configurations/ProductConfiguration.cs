using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", table =>
        {
            table.HasCheckConstraint("CK_Products_Price_NonNegative", "[Price] >= 0");
            table.HasCheckConstraint("CK_Products_StockQuantity_NonNegative", "[StockQuantity] >= 0");
        });

        builder.Property(product => product.Id).ValueGeneratedOnAdd();

        builder.Property(product => product.CategoryId).IsRequired();

        builder.Property(product => product.Name)
            .HasMaxLength(Product.NameMaxLength)
            .IsRequired();

        builder.Property(product => product.Slug)
            .HasMaxLength(Product.SlugMaxLength)
            .IsRequired();

        builder.Property(product => product.Sku)
            .HasMaxLength(Product.SkuMaxLength)
            .IsRequired();

        builder.Property(product => product.Description)
            .HasMaxLength(Product.DescriptionMaxLength)
            .IsRequired();

        builder.Property(product => product.Price)
            .HasPrecision(Product.PricePrecision, Product.PriceScale);

        builder.Property(product => product.CreatedAt).IsRequired();

        builder.Property(product => product.RowVersion).IsRowVersion();

        builder.HasIndex(product => product.Slug).IsUnique();
        builder.HasIndex(product => product.Sku).IsUnique();

        builder.HasOne(product => product.Category)
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
