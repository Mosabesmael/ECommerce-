namespace ECommerce.Application.DTOs;

public sealed record ProductDetailsDto(
    int Id,
    string Name,
    string Slug,
    string Description,
    string Sku,
    decimal Price,
    bool InStock,
    int CategoryId,
    string? CategoryName,
    int? BrandId = null,
    string? BrandName = null);
