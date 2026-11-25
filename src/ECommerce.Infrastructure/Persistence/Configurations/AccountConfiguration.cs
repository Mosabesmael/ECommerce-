using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Customer>, IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.Property(customer => customer.Id).ValueGeneratedNever();
        builder.Property(customer => customer.Email).HasMaxLength(254);
        builder.Property(customer => customer.NormalizedEmail).HasMaxLength(254);
        builder.HasIndex(customer => customer.NormalizedEmail).IsUnique();
        builder.Property(customer => customer.PasswordHash).HasMaxLength(512);
    }

    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");
        builder.Property(address => address.Id).ValueGeneratedNever();
        builder.HasOne<Customer>().WithMany().HasForeignKey(address => address.CustomerId);
        builder.Property(address => address.RecipientName).HasMaxLength(120);
        builder.Property(address => address.Line1).HasMaxLength(200);
        builder.Property(address => address.Line2).HasMaxLength(200);
        builder.Property(address => address.City).HasMaxLength(100);
        builder.Property(address => address.Region).HasMaxLength(100);
        builder.Property(address => address.PostalCode).HasMaxLength(20);
        builder.Property(address => address.CountryCode).HasMaxLength(2);
    }
}
