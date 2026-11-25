namespace ECommerce.Domain.Entities;

public sealed class OrderItem
{
    public int ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem() { }

    internal OrderItem(CartItem item)
    {
        ProductId = item.ProductId;
        ProductName = item.Product.Name;
        UnitPrice = item.Product.Price;
        Quantity = item.Quantity;
    }
}
