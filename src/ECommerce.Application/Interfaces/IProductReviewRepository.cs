using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IProductReviewRepository
{
    Task<bool> IsProductActiveAsync(int productId, CancellationToken cancellationToken);
    Task<bool> HasPurchasedAsync(int productId, Guid customerId, CancellationToken cancellationToken);
    Task<PagedResult<ProductReviewDto>> GetPageAsync(int productId, ProductReviewQueryParameters query,
        CancellationToken cancellationToken);
    Task<ProductReview> SaveAsync(int productId, Guid customerId, int rating, string? comment,
        CancellationToken cancellationToken);
}
