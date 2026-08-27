using MagMini.Application.Common.Models;
using MagMini.Application.DTOs.Articles;
using MagMini.Domain.Entities;

namespace MagMini.Application.Common.Interfaces;

public interface IArticleService
{
    Task<PagedResult<ArticleDto>> GetPagedAsync(ArticleFilterDto filter, CancellationToken cancellationToken = default);
    Task<SaveArticleDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Category>> GetCategoriesLookupAsync(CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> SaveAsync(SaveArticleDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}