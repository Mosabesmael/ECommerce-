using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;

namespace ECommerce.Application.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        return categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Slug,
            c.ParentId
        )).ToList();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return null;
        }

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.ParentId
        );
    }
}
