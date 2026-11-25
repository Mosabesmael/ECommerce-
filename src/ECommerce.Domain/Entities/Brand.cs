using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public sealed class Brand
{
    public const int NameMaxLength = 200;
    public const int SlugMaxLength = 200;

    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;

    private Brand()
    {
    }

    public static Brand Create(string name, string slug) => new()
    {
        Name = RequiredText(name, nameof(name), NameMaxLength),
        Slug = RequiredText(slug, nameof(slug), SlugMaxLength)
    };

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
