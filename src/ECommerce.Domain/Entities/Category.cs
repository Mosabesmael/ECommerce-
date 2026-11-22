using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public sealed class Category
{
    public const int NameMaxLength = 200;
    public const int SlugMaxLength = 200;

    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public int? ParentId { get; private set; }
    public Category? Parent { get; private set; }

    private Category()
    {
    }

    public static Category Create(string name, string slug, int? parentId = null)
    {
        if (parentId is <= 0)
        {
            throw new DomainException("Parent category id must be a positive number.");
        }

        return new Category
        {
            Name = RequiredText(name, nameof(name), NameMaxLength),
            Slug = RequiredText(slug, nameof(slug), SlugMaxLength),
            ParentId = parentId
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
