using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public sealed class ProductReviewRepository(AppDbContext dbContext) : IProductReviewRepository
{
    public Task<bool> IsProductActiveAsync(int productId, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(product => product.Id == productId && product.IsActive, cancellationToken);

    public Task<bool> HasPurchasedAsync(int productId, Guid customerId, CancellationToken cancellationToken) =>
        dbContext.Set<Order>().AnyAsync(order => order.CustomerId == customerId &&
            order.Items.Any(item => item.ProductId == productId), cancellationToken);

    public async Task<PagedResult<ProductReviewDto>> GetPageAsync(int productId,
        ProductReviewQueryParameters query, CancellationToken cancellationToken)
    {
        var reviews = dbContext.Set<ProductReview>().AsNoTracking().Where(review => review.ProductId == productId);
        var totalCount = await reviews.CountAsync(cancellationToken);
        var offset = (query.PageNumber - 1L) * query.PageSize;
        if (offset >= totalCount)
        {
            return new([], totalCount, query.PageNumber, query.PageSize);
        }

        var items = await reviews.OrderByDescending(review => review.CreatedAt).ThenBy(review => review.CustomerId)
            .Skip((int)offset).Take(query.PageSize)
            .Select(review => new ProductReviewDto(review.Rating, review.Comment, review.CreatedAt, review.UpdatedAt))
            .ToListAsync(cancellationToken);
        return new(items, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task<ProductReview> SaveAsync(int productId, Guid customerId, int rating, string? comment,
        CancellationToken cancellationToken)
    {
        var reviews = dbContext.Set<ProductReview>();
        object[] key = [productId, customerId];
        var review = await reviews.FindAsync(key, cancellationToken);
        if (review is not null)
        {
            review.Update(rating, comment);
            await dbContext.SaveChangesAsync(cancellationToken);
            return review;
        }

        var newReview = new ProductReview(productId, customerId, rating, comment);
        reviews.Add(newReview);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return newReview;
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // A concurrent request inserted this customer's review first.
            dbContext.Entry(newReview).State = EntityState.Detached;
            review = await reviews.FindAsync(key, cancellationToken);
            if (review is null)
            {
                throw;
            }

            review.Update(rating, comment);
            await dbContext.SaveChangesAsync(cancellationToken);
            return review;
        }
    }
}
