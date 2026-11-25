using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.DTOs;

public sealed class ProductReviewQueryParameters
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}

public sealed class SaveProductReviewRequest
{
    [Range(ProductReview.MinRating, ProductReview.MaxRating)]
    public int Rating { get; set; }

    [StringLength(ProductReview.CommentMaxLength)]
    public string? Comment { get; set; }
}

public sealed record ProductReviewDto(int Rating, string? Comment, DateTime CreatedAt, DateTime UpdatedAt);
