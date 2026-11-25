namespace ECommerce.Application.DTOs;

public sealed record CategoryDto(
    int Id,
    string Name,
    string Slug,
    int? ParentId);
