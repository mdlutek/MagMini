using MagMini.Application.DTOs.Categories;

namespace MagMini.Application.Common.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(string? searchPhrase = null, CancellationToken cancellationToken = default);
    Task<SaveCategoryDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> SaveAsync(SaveCategoryDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}