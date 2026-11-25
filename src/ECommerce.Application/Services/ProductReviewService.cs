using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;

namespace ECommerce.Application.Services;

public sealed class ProductReviewService(IProductReviewRepository repository)
{
    public async Task<PagedResult<ProductReviewDto>?> GetPageAsync(int productId,
        ProductReviewQueryParameters query, CancellationToken cancellationToken)
    {
        if (!await repository.IsProductActiveAsync(productId, cancellationToken))
        {
            return null;
        }

        return await repository.GetPageAsync(productId, query, cancellationToken);
    }

    public async Task<SaveProductReviewResult> SaveAsync(int productId, Guid customerId,
        SaveProductReviewRequest request, CancellationToken cancellationToken)
    {
        if (!await repository.IsProductActiveAsync(productId, cancellationToken))
        {
            return new(SaveProductReviewStatus.ProductNotFound);
        }

        if (!await repository.HasPurchasedAsync(productId, customerId, cancellationToken))
        {
            return new(SaveProductReviewStatus.PurchaseRequired);
        }

        var review = await repository.SaveAsync(productId, customerId, request.Rating, request.Comment,
            cancellationToken);
        return new(SaveProductReviewStatus.Saved,
            new ProductReviewDto(review.Rating, review.Comment, review.CreatedAt, review.UpdatedAt));
    }
}

public enum SaveProductReviewStatus
{
    Saved,
    ProductNotFound,
    PurchaseRequired
}

public sealed record SaveProductReviewResult(SaveProductReviewStatus Status, ProductReviewDto? Review = null);
