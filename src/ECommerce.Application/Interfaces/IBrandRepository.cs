using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IBrandRepository
{
    Task<IReadOnlyList<Brand>> GetAllAsync(CancellationToken cancellationToken);
    Task<Brand?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
