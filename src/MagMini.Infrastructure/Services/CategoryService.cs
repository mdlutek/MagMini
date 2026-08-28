using MagMini.Application.Common.Interfaces;
using MagMini.Application.DTOs.Categories;
using MagMini.Domain.Entities;
using MagMini.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MagMini.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> GetAllAsync(string? searchPhrase = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories
            .AsNoTracking()
            .Include(c => c.Articles)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchPhrase))
        {
            var phrase = searchPhrase.Trim().ToLower();
            query = query.Where(c => c.Code.ToLower().Contains(phrase) || c.Name.ToLower().Contains(phrase));
        }

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                ArticlesCount = c.Articles.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SaveCategoryDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FindAsync([id], cancellationToken);
        if (category == null) return null;

        return new SaveCategoryDto
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task<(bool Success, string? Error)> SaveAsync(SaveCategoryDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
            return (false, "Kod i Nazwa kategorii są wymagane.");

        var codeExists = await _context.Categories
            .AnyAsync(c => c.Code.ToLower() == dto.Code.Trim().ToLower() && c.Id != dto.Id, cancellationToken);

        if (codeExists)
            return (false, $"Kategoria o kodzie '{dto.Code}' już istnieje.");

        if (dto.Id == 0)
        {
            var category = new Category
            {
                Code = dto.Code.Trim().ToUpper(),
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim()
            };
            await _context.Categories.AddAsync(category, cancellationToken);
        }
        else
        {
            var category = await _context.Categories.FindAsync([dto.Id], cancellationToken);
            if (category == null) return (false, "Nie odnaleziono kategorii.");

            category.Code = dto.Code.Trim().ToUpper();
            category.Name = dto.Name.Trim();
            category.Description = dto.Description?.Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var hasArticles = await _context.Articles.AnyAsync(a => a.CategoryId == id, cancellationToken);
        if (hasArticles)
            return (false, "Nie można usunąć kategorii, do której przypisane są towary w kartotece.");

        var category = await _context.Categories.FindAsync([id], cancellationToken);
        if (category == null) return (false, "Nie znaleziono kategorii.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}