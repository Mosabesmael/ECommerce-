namespace ECommerce.Domain.Entities;

public sealed class CartItem
{
    public int ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Quantity { get; internal set; }

    private CartItem() { }

    internal CartItem(Product product, int quantity)
    {
        ProductId = product.Id;
        Product = product;
        Quantity = quantity;
    }
}
