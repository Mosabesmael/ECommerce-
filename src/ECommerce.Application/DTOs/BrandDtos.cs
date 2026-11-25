namespace ECommerce.Application.DTOs;

public sealed record BrandDto(int Id, string Name, string Slug);

public sealed record CreateBrandRequest(string Name, string Slug);
