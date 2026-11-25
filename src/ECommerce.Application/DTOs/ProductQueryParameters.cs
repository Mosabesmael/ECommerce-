using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs;

public sealed class ProductQueryParameters : IValidatableObject
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;
    private const int DefaultPageNumber = 1;

    private string? _searchTerm;

    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than or equal to 1.")]
    public int PageNumber { get; set; } = DefaultPageNumber;

    [Range(1, MaxPageSize, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = DefaultPageSize;

    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be a positive integer.")]
    public int? CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "BrandId must be a positive integer.")]
    public int? BrandId { get; set; }

    [Range(0, (double)decimal.MaxValue, ErrorMessage = "MinPrice cannot be negative.")]
    public decimal? MinPrice { get; set; }

    [Range(0, (double)decimal.MaxValue, ErrorMessage = "MaxPrice cannot be negative.")]
    public decimal? MaxPrice { get; set; }

    public string? Search
    {
        get => _searchTerm;
        set => _searchTerm = value;
    }

    public string? SearchTerm
    {
        get => _searchTerm;
        set => _searchTerm = value;
    }

    public string? SortBy { get; set; }

    public string? SortOrder { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice.Value > MaxPrice.Value)
        {
            yield return new ValidationResult(
                "MinPrice cannot be greater than MaxPrice.",
                new[] { nameof(MinPrice), nameof(MaxPrice) });
        }
    }
}
