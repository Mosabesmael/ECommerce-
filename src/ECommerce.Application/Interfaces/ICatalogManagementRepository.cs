using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ICatalogManagementRepository
{
    void Add(Brand brand);
    void Add(Category category);
    void Add(Product product);
    Task<Product?> GetProductAsync(int id, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
}
