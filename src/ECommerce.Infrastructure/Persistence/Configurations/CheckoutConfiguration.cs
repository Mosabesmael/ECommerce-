using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

internal sealed class CheckoutConfiguration : IEntityTypeConfiguration<Cart>, IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        builder.Property(cart => cart.Id).ValueGeneratedNever();
        builder.Property(cart => cart.Revision).IsConcurrencyToken();
        builder.HasOne<Customer>().WithMany().HasForeignKey(cart => cart.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(cart => cart.Order).WithOne().HasForeignKey<Order>(order => order.CartId);
        builder.OwnsMany(cart => cart.Items, items =>
        {
            items.ToTable("CartItems");
            items.WithOwner().HasForeignKey("CartId");
            items.HasKey("CartId", nameof(CartItem.ProductId));
            items.HasOne(item => item.Product).WithMany().HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.Property(order => order.Id).ValueGeneratedNever();
        builder.Property(order => order.Total).HasPrecision(28, 2);
        builder.Property(order => order.Discount).HasPrecision(28, 2);
        builder.Property(order => order.CouponCode).HasMaxLength(Coupon.MaxCodeLength);
        builder.HasOne<Customer>().WithMany().HasForeignKey(order => order.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(order => new { order.CustomerId, order.CreatedAt, order.Id });
        builder.OwnsOne(order => order.ShippingAddress);
        builder.OwnsMany(order => order.Items, items =>
        {
            items.ToTable("OrderItems");
            items.WithOwner().HasForeignKey("OrderId");
            items.HasKey("OrderId", nameof(OrderItem.ProductId));
            items.Property(item => item.ProductId).ValueGeneratedNever();
            items.Property(item => item.ProductName).HasMaxLength(Product.NameMaxLength);
            items.Property(item => item.UnitPrice).HasPrecision(Product.PricePrecision, Product.PriceScale);
        });
    }
}
