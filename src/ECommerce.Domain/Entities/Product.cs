using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public sealed class Product
{
    public const int NameMaxLength = 200;
    public const int SlugMaxLength = 200;
    public const int SkuMaxLength = 64;
    public const int DescriptionMaxLength = 4000;
    public const int PricePrecision = 18;
    public const int PriceScale = 2;
    public const decimal MaxPrice = 9_999_999_999_999_999.99m;

    public int Id { get; private set; }
    public int CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public int? BrandId { get; private set; }
    public Brand? Brand { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Sku { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = null!;

    private Product()
    {
    }

    public void ReserveStock(int quantity)
    {
        if (!IsActive || quantity <= 0 || quantity > StockQuantity)
        {
            throw new DomainException("The requested quantity is unavailable.");
        }

        StockQuantity -= quantity;
    }

    public static Product Create(
        int categoryId,
        string name,
        string slug,
        string description,
        string sku,
        decimal price,
        int stockQuantity,
        bool isActive = true,
        int? brandId = null)
    {
        var product = new Product { CreatedAt = DateTime.UtcNow };
        product.Update(categoryId, name, slug, description, sku, price, stockQuantity, isActive, brandId);
        return product;
    }

    public void Update(int categoryId, string name, string slug, string description, string sku,
        decimal price, int stockQuantity, bool isActive, int? brandId = null)
    {
        if (categoryId <= 0)
        {
            throw new DomainException("Category id must be a positive number.");
        }

        if (brandId is <= 0)
        {
            throw new DomainException("Brand id must be a positive number.");
        }

        if (price < 0 || price > MaxPrice)
        {
            throw new DomainException($"Price must be between 0 and {MaxPrice}.");
        }

        if (decimal.Round(price, PriceScale) != price)
        {
            throw new DomainException($"Price cannot have more than {PriceScale} decimal places.");
        }

        if (stockQuantity < 0)
        {
            throw new DomainException("Stock quantity cannot be negative.");
        }

        if (description is null)
        {
            throw new DomainException("Description is required.");
        }

        var trimmedDescription = description.Trim();
        if (trimmedDescription.Length > DescriptionMaxLength)
        {
            throw new DomainException($"Description cannot exceed {DescriptionMaxLength} characters.");
        }

        var validatedName = RequiredText(name, nameof(name), NameMaxLength);
        var validatedSlug = RequiredText(slug, nameof(slug), SlugMaxLength);
        var validatedSku = RequiredText(sku, nameof(sku), SkuMaxLength);

        CategoryId = categoryId;
        BrandId = brandId;
        Name = validatedName;
        Slug = validatedSlug;
        Description = trimmedDescription;
        Sku = validatedSku;
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = isActive;
    }

    private static string RequiredText(string value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{name} is required.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{name} cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }
}
