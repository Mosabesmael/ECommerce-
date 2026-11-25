using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

internal sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");

        builder.Property(brand => brand.Id).ValueGeneratedOnAdd();

        builder.Property(brand => brand.Name)
            .HasMaxLength(Brand.NameMaxLength)
            .IsRequired();

        builder.Property(brand => brand.Slug)
            .HasMaxLength(Brand.SlugMaxLength)
            .IsRequired();

        builder.HasIndex(brand => brand.Slug).IsUnique();
    }
}
