using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

internal sealed class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("ProductReviews");
        builder.HasKey(review => new { review.ProductId, review.CustomerId });
        builder.Property(review => review.Comment).HasMaxLength(ProductReview.CommentMaxLength);
        builder.HasOne<Product>().WithMany().HasForeignKey(review => review.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Customer>().WithMany().HasForeignKey(review => review.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(review => new { review.ProductId, review.CreatedAt, review.CustomerId })
            .IsDescending(false, true, false);
    }
}
