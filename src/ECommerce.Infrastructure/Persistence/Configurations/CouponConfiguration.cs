using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

internal sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons", table => table.HasCheckConstraint("CK_Coupons_PercentOff", "[PercentOff] BETWEEN 1 AND 100"));
        builder.HasKey(coupon => coupon.Code);
        builder.Property(coupon => coupon.Code).HasMaxLength(Coupon.MaxCodeLength).IsUnicode(false);
    }
}
