using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
}
