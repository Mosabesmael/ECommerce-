using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Application.Services;

public sealed class CheckoutService(ICheckoutRepository repository, IAccountRepository accounts)
{
    public async Task<CartDto> CreateAsync(Guid? customerId, CancellationToken cancellationToken)
    {
        var cart = new Cart(customerId);
        repository.Add(cart);
        await repository.SaveAsync(cancellationToken);
        return ToDto(cart);
    }

    public async Task<CartDto?> GetAsync(Guid id, Guid? customerId, CancellationToken cancellationToken)
    {
        var cart = await repository.GetCartAsync(id, customerId, cancellationToken);
        return cart is null ? null : ToDto(cart);
    }

    public async Task<CartDto?> SetQuantityAsync(Guid id, Guid? customerId, int productId, int quantity, CancellationToken cancellationToken)
    {
        var cart = await repository.GetCartAsync(id, customerId, cancellationToken);
        if (cart is null || await repository.GetProductAsync(productId, cancellationToken) is not { } product)
        {
            return null;
        }

        cart.SetQuantity(product, quantity);
        await repository.SaveAsync(cancellationToken);
        return ToDto(cart);
    }

    public async Task<OrderDto?> GetOrderAsync(Guid id, Guid? customerId, CancellationToken cancellationToken)
    {
        var cart = await repository.GetCartAsync(id, customerId, cancellationToken);
        return cart?.Order is { } order ? ToDto(order) : null;
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(Guid customerId, int offset, int limit, CancellationToken cancellationToken)
    {
        var orders = await repository.GetOrdersAsync(customerId, offset, limit, cancellationToken);
        return orders.Select(ToDto).ToList();
    }

    public async Task<OrderDto?> CheckoutAsync(Guid id, Guid? customerId, DemoCheckoutRequest request, CancellationToken cancellationToken)
    {
        var cart = await repository.GetCartAsync(id, customerId, cancellationToken);
        if (cart is null)
        {
            return null;
        }

        if (cart.Order is { } existingOrder)
        {
            return ToDto(existingOrder);
        }

        if (request.ApprovePayment != true)
        {
            throw new DomainException("The simulated payment was declined.");
        }

        ShippingAddress? shippingAddress = null;
        if (customerId is Guid owner)
        {
            if (request.AddressId is not Guid addressId ||
                await accounts.GetAddressAsync(owner, addressId, cancellationToken) is not { } address)
            {
                throw new DomainException("Choose a saved address from your account.");
            }

            shippingAddress = new ShippingAddress(address.RecipientName, address.Line1, address.Line2,
                address.City, address.Region, address.PostalCode, address.CountryCode);
        }

        Coupon? coupon = null;
        if (request.CouponCode is { } code)
        {
            coupon = await repository.GetCouponAsync(Coupon.NormalizeCode(code), cancellationToken);
            if (coupon is null || coupon.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                throw new DomainException("The coupon is invalid or expired.");
            }
        }

        var order = cart.Checkout(shippingAddress, coupon);
        await repository.SaveAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<CouponDto> CreateCouponAsync(CreateCouponRequest request, CancellationToken cancellationToken)
    {
        var coupon = Coupon.Create(request.Code, request.PercentOff, request.ExpiresAt, DateTimeOffset.UtcNow);
        if (await repository.GetCouponAsync(coupon.Code, cancellationToken) is not null)
        {
            throw new CatalogWriteConflictException("The coupon code is already in use.");
        }

        repository.Add(coupon);
        await repository.SaveCouponAsync(cancellationToken);
        return new CouponDto(coupon.Code, coupon.PercentOff, coupon.ExpiresAt);
    }

    private static CartDto ToDto(Cart cart) => new(cart.Id, cart.Order is not null,
        cart.Items.OrderBy(item => item.ProductId).Select(item =>
            new CartItemDto(item.ProductId, item.Product.Name, item.Product.Price, item.Quantity)).ToList());

    private static OrderDto ToDto(Order order) => new(order.Id, order.DemoPaymentReference, order.CreatedAt, order.Total,
        order.Items.OrderBy(item => item.ProductId).Select(item =>
            new CartItemDto(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity)).ToList(),
        order.Total + order.Discount, order.Discount, order.CouponCode, order.ShippingAddress);
}
