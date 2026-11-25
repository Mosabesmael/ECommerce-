namespace ECommerce.Application.DTOs;

public sealed record ProductSummaryDto(
    int Id,
    string Name,
    string Slug,
    decimal Price,
    bool InStock,
    int CategoryId,
    string? CategoryName,
    int? BrandId = null,
    string? BrandName = null);
