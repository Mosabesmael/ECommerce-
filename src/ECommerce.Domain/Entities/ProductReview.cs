using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public sealed class ProductReview
{
    public const int MinRating = 1;
    public const int MaxRating = 5;
    public const int CommentMaxLength = 2000;

    public int ProductId { get; private set; }
    public Guid CustomerId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private ProductReview() { }

    public ProductReview(int productId, Guid customerId, int rating, string? comment)
    {
        ProductId = productId;
        CustomerId = customerId;
        Update(rating, comment);
        CreatedAt = UpdatedAt;
    }

    public void Update(int rating, string? comment)
    {
        if (rating is < MinRating or > MaxRating)
        {
            throw new DomainException($"Rating must be between {MinRating} and {MaxRating}.");
        }

        if (comment?.Length > CommentMaxLength)
        {
            throw new DomainException($"Comment cannot exceed {CommentMaxLength} characters.");
        }

        Rating = rating;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
