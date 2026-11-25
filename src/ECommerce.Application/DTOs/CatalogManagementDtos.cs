using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs;

public sealed record CreateCategoryRequest(string Name, string Slug, int? ParentId);

public class CreateProductRequest
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }

    [Range(1, int.MaxValue)]
    public int? BrandId { get; init; }

    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required string Description { get; init; }
    public required string Sku { get; init; }
    public required decimal Price { get; init; }
    public required int StockQuantity { get; init; }
    public required bool IsActive { get; init; }
}

public sealed class UpdateProductRequest : CreateProductRequest
{
    [StringLength(12, MinimumLength = 12)]
    public required string Version { get; init; }
}

public sealed record ManagedProductDto(int Id, int CategoryId, string Name, string Slug, string Description,
    string Sku, decimal Price, int StockQuantity, bool IsActive, string Version, int? BrandId = null);
