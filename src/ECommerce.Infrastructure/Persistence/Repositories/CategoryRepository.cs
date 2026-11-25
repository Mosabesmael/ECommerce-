using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _dbContext;

    public CategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
