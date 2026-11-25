using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductSummaryDto>> GetActiveProductsAsync(ProductQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<ProductDetailsDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
}
