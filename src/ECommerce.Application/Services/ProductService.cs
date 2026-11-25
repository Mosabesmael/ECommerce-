using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;

namespace ECommerce.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedResult<ProductSummaryDto>> GetActiveProductsAsync(
        ProductQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new ProductQueryParameters();

        var pagedProducts = await _productRepository.GetPagedActiveAsync(parameters, cancellationToken);

        var dtos = pagedProducts.Items.Select(p => new ProductSummaryDto(
            p.Id,
            p.Name,
            p.Slug,
            p.Price,
            p.StockQuantity > 0,
            p.CategoryId,
            p.Category?.Name,
            p.BrandId,
            p.Brand?.Name
        )).ToList();

        return new PagedResult<ProductSummaryDto>(
            dtos,
            pagedProducts.TotalCount,
            pagedProducts.PageNumber,
            pagedProducts.PageSize
        );
    }

    public async Task<ProductDetailsDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        return new ProductDetailsDto(
            product.Id,
            product.Name,
            product.Slug,
            product.Description,
            product.Sku,
            product.Price,
            product.StockQuantity > 0,
            product.CategoryId,
            product.Category?.Name,
            product.BrandId,
            product.Brand?.Name
        );
    }
}
