using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

internal sealed class AccountRepository(AppDbContext dbContext) : IAccountRepository
{
    public Task<Customer?> FindCustomerAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        dbContext.Set<Customer>().AsNoTracking()
            .SingleOrDefaultAsync(customer => customer.NormalizedEmail == normalizedEmail, cancellationToken);

    public async Task<bool> AddCustomerAsync(Customer customer, CancellationToken cancellationToken)
    {
        dbContext.Set<Customer>().Add(customer);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<Address>> GetAddressesAsync(Guid customerId, CancellationToken cancellationToken) =>
        await dbContext.Set<Address>().AsNoTracking().Where(address => address.CustomerId == customerId)
            .OrderBy(address => address.RecipientName).ThenBy(address => address.Id).ToListAsync(cancellationToken);

    public Task<Address?> GetAddressAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken) =>
        dbContext.Set<Address>().SingleOrDefaultAsync(address => address.CustomerId == customerId
            && address.Id == addressId, cancellationToken);

    public async Task AddAddressAsync(Address address, CancellationToken cancellationToken)
    {
        dbContext.Set<Address>().Add(address);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SaveAddressAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    public Task<bool> RemoveAddressAsync(Address address, CancellationToken cancellationToken)
    {
        dbContext.Set<Address>().Remove(address);
        return SaveAddressAsync(cancellationToken);
    }
}
