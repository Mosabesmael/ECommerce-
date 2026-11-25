using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.DTOs;

public sealed record CartItemDto(int ProductId, string Name, decimal UnitPrice, int Quantity);
public sealed record CartDto(Guid Id, bool CheckedOut, IReadOnlyList<CartItemDto> Items);
public sealed record OrderDto(Guid Id, Guid DemoPaymentReference, DateTime CreatedAt, decimal Total,
    IReadOnlyList<CartItemDto> Items, decimal Subtotal, decimal Discount, string? CouponCode, ShippingAddress? ShippingAddress);

public sealed class SetCartQuantityRequest
{
    [Required, Range(0, Cart.MaxQuantity)]
    public int? Quantity { get; init; }
}

public sealed class DemoCheckoutRequest
{
    [Required]
    public bool? ApprovePayment { get; init; }

    public Guid? AddressId { get; init; }

    [StringLength(Coupon.MaxCodeLength)]
    public string? CouponCode { get; init; }
}

public sealed record CreateCouponRequest([Required, StringLength(Coupon.MaxCodeLength)] string Code,
    [Range(1, 100)] int PercentOff, DateTimeOffset ExpiresAt);

public sealed record CouponDto(string Code, int PercentOff, DateTimeOffset ExpiresAt);
