using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public sealed class CheckoutRepository(AppDbContext dbContext) : ICheckoutRepository
{
    public void Add(Cart cart) => dbContext.Set<Cart>().Add(cart);

    public void Add(Coupon coupon) => dbContext.Set<Coupon>().Add(coupon);

    public async Task SaveCouponAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new CatalogWriteConflictException("The coupon code is already in use.");
        }
    }

    public Task<Coupon?> GetCouponAsync(string code, CancellationToken cancellationToken) => dbContext.Set<Coupon>()
        .AsNoTracking().SingleOrDefaultAsync(coupon => coupon.Code == code, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetOrdersAsync(Guid customerId, int offset, int limit, CancellationToken cancellationToken) =>
        await dbContext.Set<Order>().AsNoTracking().Include(order => order.Items)
            .Where(order => order.CustomerId == customerId).OrderByDescending(order => order.CreatedAt)
            .ThenByDescending(order => order.Id).Skip(offset).Take(limit).ToListAsync(cancellationToken);

    public Task<Cart?> GetCartAsync(Guid id, Guid? customerId, CancellationToken cancellationToken) => dbContext.Set<Cart>()
        .Include(cart => cart.Items).ThenInclude(item => item.Product)
        .Include(cart => cart.Order).ThenInclude(order => order!.Items)
        .AsSingleQuery()
        .FirstOrDefaultAsync(cart => cart.Id == id && cart.CustomerId == customerId, cancellationToken);

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
            throw new DomainException("The cart or stock changed. Reload and retry.");
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 or 1205 })
        {
            throw new DomainException("The cart or stock changed. Reload and retry.");
        }
    }
}
