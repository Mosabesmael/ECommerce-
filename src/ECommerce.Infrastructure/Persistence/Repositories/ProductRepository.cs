using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;

    public ProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Product>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Product>> GetPagedActiveAsync(
        ProductQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new ProductQueryParameters();

        int pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        int pageSize = parameters.PageSize < 1 ? 10 : (parameters.PageSize > 100 ? 100 : parameters.PageSize);

        var query = _dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (parameters.CategoryId.HasValue && parameters.CategoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == parameters.CategoryId.Value);
        }

        if (parameters.BrandId is int brandId && brandId > 0)
        {
            query = query.Where(p => p.BrandId == brandId);
        }

        if (parameters.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= parameters.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            string searchTerm = parameters.SearchTerm.Trim();
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{searchTerm}%"));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        long offset = (pageNumber - 1L) * pageSize;
        if (offset >= totalCount)
        {
            return new PagedResult<Product>(Array.Empty<Product>(), totalCount, pageNumber, pageSize);
        }

        bool isDescending = string.Equals(parameters.SortOrder?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);

        string normalizedSortBy = parameters.SortBy?.Trim().ToLowerInvariant() switch
        {
            "price" => "price",
            "createdat" or "created_at" => "createdat",
            _ => "name"
        };

        query = normalizedSortBy switch
        {
            "price" => isDescending
                ? query.OrderByDescending(p => p.Price).ThenByDescending(p => p.Id)
                : query.OrderBy(p => p.Price).ThenBy(p => p.Id),
            "createdat" => isDescending
                ? query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
                : query.OrderBy(p => p.CreatedAt).ThenBy(p => p.Id),
            _ => isDescending
                ? query.OrderByDescending(p => p.Name).ThenByDescending(p => p.Id)
                : query.OrderBy(p => p.Name).ThenBy(p => p.Id)
        };

        var items = await query
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Skip((int)offset)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);
    }
}
