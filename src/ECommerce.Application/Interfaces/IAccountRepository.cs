using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IAccountRepository
{
    Task<Customer?> FindCustomerAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<bool> AddCustomerAsync(Customer customer, CancellationToken cancellationToken);
    Task<IReadOnlyList<Address>> GetAddressesAsync(Guid customerId, CancellationToken cancellationToken);
    Task<Address?> GetAddressAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken);
    Task AddAddressAsync(Address address, CancellationToken cancellationToken);
    Task<bool> SaveAddressAsync(CancellationToken cancellationToken);
    Task<bool> RemoveAddressAsync(Address address, CancellationToken cancellationToken);
}
