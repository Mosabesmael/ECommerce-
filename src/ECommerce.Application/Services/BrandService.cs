using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public sealed class BrandService(IBrandRepository brands, ICatalogManagementRepository catalog)
{
    public async Task<IReadOnlyList<BrandDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await brands.GetAllAsync(cancellationToken);
        return items.Select(ToDto).ToList();
    }

    public async Task<BrandDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var brand = await brands.GetByIdAsync(id, cancellationToken);
        return brand is null ? null : ToDto(brand);
    }

    public async Task<BrandDto> CreateAsync(CreateBrandRequest request, CancellationToken cancellationToken)
    {
        var brand = Brand.Create(request.Name, request.Slug);
        catalog.Add(brand);
        await catalog.SaveAsync(cancellationToken);
        return ToDto(brand);
    }

    private static BrandDto ToDto(Brand brand) => new(brand.Id, brand.Name, brand.Slug);
}
