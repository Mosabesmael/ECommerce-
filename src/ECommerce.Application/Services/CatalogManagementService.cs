using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Application.Services;

public sealed class CatalogManagementService(ICatalogManagementRepository repository, ICategoryRepository categories,
    IBrandRepository brands)
{
    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = Category.Create(request.Name, request.Slug, request.ParentId);
        if (category.ParentId is int parentId)
        {
            await RequireCategoryAsync(parentId, cancellationToken);
        }

        repository.Add(category);
        await repository.SaveAsync(cancellationToken);
        return new CategoryDto(category.Id, category.Name, category.Slug, category.ParentId);
    }

    public async Task<ManagedProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.CategoryId, request.Name, request.Slug, request.Description,
            request.Sku, request.Price, request.StockQuantity, request.IsActive, request.BrandId);
        await RequireCategoryAsync(request.CategoryId, cancellationToken);
        await RequireBrandAsync(request.BrandId, cancellationToken);
        repository.Add(product);
        await repository.SaveAsync(cancellationToken);
        return ToDto(product);
    }

    public async Task<ManagedProductDto?> GetProductAsync(int id, CancellationToken cancellationToken)
    {
        var product = await repository.GetProductAsync(id, cancellationToken);
        return product is null ? null : ToDto(product);
    }

    public async Task<ManagedProductDto?> UpdateProductAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await repository.GetProductAsync(id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        if (Convert.ToBase64String(product.RowVersion) != request.Version)
        {
            throw new CatalogWriteConflictException("The product changed. Reload it before updating.");
        }

        product.Update(request.CategoryId, request.Name, request.Slug, request.Description,
            request.Sku, request.Price, request.StockQuantity, request.IsActive, request.BrandId);
        await RequireCategoryAsync(request.CategoryId, cancellationToken);
        await RequireBrandAsync(request.BrandId, cancellationToken);
        await repository.SaveAsync(cancellationToken);
        return ToDto(product);
    }

    private async Task RequireCategoryAsync(int id, CancellationToken cancellationToken)
    {
        if (await categories.GetByIdAsync(id, cancellationToken) is null)
        {
            throw new DomainException("The specified category does not exist.");
        }
    }

    private async Task RequireBrandAsync(int? id, CancellationToken cancellationToken)
    {
        if (id is int brandId && await brands.GetByIdAsync(brandId, cancellationToken) is null)
        {
            throw new DomainException("The specified brand does not exist.");
        }
    }

    private static ManagedProductDto ToDto(Product product) => new(product.Id, product.CategoryId, product.Name,
        product.Slug, product.Description, product.Sku, product.Price, product.StockQuantity, product.IsActive,
        Convert.ToBase64String(product.RowVersion), product.BrandId);
}
