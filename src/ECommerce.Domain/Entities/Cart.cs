using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public sealed class Cart
{
    public const int MaxQuantity = 1000;
    private readonly List<CartItem> _items = [];

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? CustomerId { get; private set; }
    public Guid Revision { get; private set; } = Guid.NewGuid();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
    public Order? Order { get; private set; }

    private Cart() { }

    public Cart(Guid? customerId) => CustomerId = customerId;

    public void SetQuantity(Product product, int quantity)
    {
        if (Order is not null)
        {
            throw new DomainException("The cart has already been checked out.");
        }

        var item = _items.SingleOrDefault(item => item.ProductId == product.Id);
        if (quantity == 0)
        {
            if (item is not null)
            {
                _items.Remove(item);
            }
        }
        else
        {
            if (!product.IsActive || quantity < 0 || quantity > MaxQuantity || quantity > product.StockQuantity)
            {
                throw new DomainException("The requested quantity is unavailable.");
            }

            if (item is null)
            {
                if (_items.Count >= 100)
                {
                    throw new DomainException("A cart can contain at most 100 different products.");
                }

                _items.Add(new CartItem(product, quantity));
            }
            else
            {
                item.Quantity = quantity;
            }
        }

        Revision = Guid.NewGuid();
    }

    public Order Checkout(ShippingAddress? shippingAddress, Coupon? coupon)
    {
        if (Order is not null)
        {
            return Order;
        }

        if (_items.Count == 0)
        {
            throw new DomainException("The cart is empty.");
        }

        if (CustomerId.HasValue && shippingAddress is null)
        {
            throw new DomainException("Choose a saved shipping address.");
        }

        if (_items.Any(item => !item.Product.IsActive || item.Quantity > item.Product.StockQuantity))
        {
            throw new DomainException("One or more products are unavailable in the requested quantity.");
        }

        foreach (var item in _items)
        {
            item.Product.ReserveStock(item.Quantity);
        }

        Order = new Order(Id, CustomerId, _items, shippingAddress, coupon);
        Revision = Guid.NewGuid();
        return Order;
    }
}
