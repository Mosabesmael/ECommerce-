using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
