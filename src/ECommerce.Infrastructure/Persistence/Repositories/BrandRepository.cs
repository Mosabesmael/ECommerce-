using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public sealed class BrandRepository(AppDbContext dbContext) : IBrandRepository
{
    public async Task<IReadOnlyList<Brand>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Brands.AsNoTracking()
            .OrderBy(brand => brand.Name)
            .ThenBy(brand => brand.Id)
            .ToListAsync(cancellationToken);

    public Task<Brand?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Brands.AsNoTracking().SingleOrDefaultAsync(brand => brand.Id == id, cancellationToken);
}
