using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<Product>> GetPagedActiveAsync(ProductQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
