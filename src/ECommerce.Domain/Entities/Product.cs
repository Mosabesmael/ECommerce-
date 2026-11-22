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

    public int Id { get; private set; }
    public int CategoryId { get; private set; }
    public Category? Category { get; private set; }
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

    public static Product Create(
        int categoryId,
        string name,
        string slug,
        string description,
        string sku,
        decimal price,
        int stockQuantity,
        bool isActive = true)
    {
        if (categoryId <= 0)
        {
            throw new DomainException("Category id must be a positive number.");
        }

        if (price < 0)
        {
            throw new DomainException("Price cannot be negative.");
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

        return new Product
        {
            CategoryId = categoryId,
            Name = RequiredText(name, nameof(name), NameMaxLength),
            Slug = RequiredText(slug, nameof(slug), SlugMaxLength),
            Description = trimmedDescription,
            Sku = RequiredText(sku, nameof(sku), SkuMaxLength),
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
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
