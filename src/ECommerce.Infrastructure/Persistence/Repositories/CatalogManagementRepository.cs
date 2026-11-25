using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public sealed class CatalogManagementRepository(AppDbContext dbContext) : ICatalogManagementRepository
{
    public void Add(Brand brand) => dbContext.Brands.Add(brand);

    public void Add(Category category) => dbContext.Categories.Add(category);

    public void Add(Product product) => dbContext.Products.Add(product);

    public Task<Product?> GetProductAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Products.SingleOrDefaultAsync(product => product.Id == id, cancellationToken);

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new CatalogWriteConflictException("The product changed. Reload it before updating.");
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            if (error.Entries.Any(entry => entry.Entity is Brand))
            {
                throw new CatalogWriteConflictException("The brand slug is already in use.");
            }

            throw new CatalogWriteConflictException("The slug or SKU is already in use.");
        }
    }
}
