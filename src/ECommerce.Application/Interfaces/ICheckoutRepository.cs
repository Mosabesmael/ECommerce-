using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ICheckoutRepository
{
    void Add(Cart cart);
    Task<Cart?> GetCartAsync(Guid id, Guid? customerId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> GetOrdersAsync(Guid customerId, int offset, int limit, CancellationToken cancellationToken);
    Task<Coupon?> GetCouponAsync(string code, CancellationToken cancellationToken);
    void Add(Coupon coupon);
    Task SaveCouponAsync(CancellationToken cancellationToken);
    Task<Product?> GetProductAsync(int id, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
}
