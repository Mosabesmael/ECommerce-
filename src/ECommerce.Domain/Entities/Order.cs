namespace ECommerce.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CartId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid DemoPaymentReference { get; private set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public decimal Total { get; private set; }
    public decimal Discount { get; private set; }
    public string? CouponCode { get; private set; }
    public ShippingAddress? ShippingAddress { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    internal Order(Guid cartId, Guid? customerId, IEnumerable<CartItem> items, ShippingAddress? shippingAddress, Coupon? coupon)
    {
        CartId = cartId;
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        CouponCode = coupon?.Code;
        _items.AddRange(items.Select(item => new OrderItem(item)));
        var subtotal = _items.Sum(item => item.UnitPrice * item.Quantity);
        Discount = coupon is null ? 0 : decimal.Round(subtotal * coupon.PercentOff / 100, 2, MidpointRounding.AwayFromZero);
        Total = subtotal - Discount;
    }
}
