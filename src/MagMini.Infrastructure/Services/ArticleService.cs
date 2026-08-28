using MagMini.Application.Common.Interfaces;
using MagMini.Application.Common.Models;
using MagMini.Application.DTOs.Articles;
using MagMini.Domain.Entities;
using MagMini.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MagMini.Infrastructure.Services;

public class ArticleService : IArticleService
{
    private readonly AppDbContext _context;

    public ArticleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ArticleDto>> GetPagedAsync(ArticleFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Articles
            .AsNoTracking()
            .Include(a => a.Category)
            .AsQueryable();

        // 1. Filtrowanie globalne po frazie (Kod, Nazwa, EAN)
        if (!string.IsNullOrWhiteSpace(filter.SearchPhrase))
        {
            var phrase = filter.SearchPhrase.Trim().ToLower();
            query = query.Where(a =>
                a.Code.ToLower().Contains(phrase) ||
                a.Name.ToLower().Contains(phrase) ||
                (a.Ean != null && a.Ean.Contains(phrase)));
        }

        // 2. Filtrowanie po kategorii
        if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
        {
            query = query.Where(a => a.CategoryId == filter.CategoryId.Value);
        }

        // 3. Pobranie łącznej liczby rekordów pasujących do filtra (szybki COUNT)
        var totalCount = await query.CountAsync(cancellationToken);

        // 4. Server-side pagination (OFFSET ... FETCH NEXT)
        var items = await query
            .OrderBy(a => a.Code)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new ArticleDto
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                Ean = a.Ean,
                CategoryId = a.CategoryId,
                CategoryName = a.Category.Name,
                Unit = a.Unit,
                VatRate = a.VatRate,
                PurchasePriceNet = a.PurchasePriceNet,
                DefaultSalePriceNet = a.DefaultSalePriceNet,
                DefaultSalePriceGross = a.DefaultSalePriceGross,
                StockQuantity = a.StockQuantity,
                MinStockQuantity = a.MinStockQuantity
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ArticleDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<SaveArticleDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var article = await _context.Articles.FindAsync([id], cancellationToken);
        if (article == null) return null;

        return new SaveArticleDto
        {
            Id = article.Id,
            Code = article.Code,
            Name = article.Name,
            Ean = article.Ean,
            CategoryId = article.CategoryId,
            Unit = article.Unit,
            VatRate = article.VatRate,
            PurchasePriceNet = article.PurchasePriceNet,
            DefaultSalePriceNet = article.DefaultSalePriceNet,
            StockQuantity = article.StockQuantity,
            MinStockQuantity = article.MinStockQuantity
        };
    }

    public async Task<List<Category>> GetCategoriesLookupAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<(bool Success, string? Error)> SaveAsync(SaveArticleDto dto, CancellationToken cancellationToken = default)
    {
        // Walidacja unikalności kodu artykułu
        var codeExists = await _context.Articles
            .AnyAsync(a => a.Code.ToLower() == dto.Code.Trim().ToLower() && a.Id != dto.Id, cancellationToken);

        if (codeExists)
        {
            return (false, $"Artykuł o kodzie '{dto.Code}' już istnieje w kartotece.");
        }

        if (dto.Id == 0)
        {
            // Nowy towar
            var article = new Article
            {
                Code = dto.Code.Trim().ToUpper(),
                Name = dto.Name.Trim(),
                Ean = string.IsNullOrWhiteSpace(dto.Ean) ? null : dto.Ean.Trim(),
                CategoryId = dto.CategoryId,
                Unit = dto.Unit,
                VatRate = dto.VatRate,
                PurchasePriceNet = dto.PurchasePriceNet,
                DefaultSalePriceNet = dto.DefaultSalePriceNet,
                StockQuantity = dto.StockQuantity,
                MinStockQuantity = dto.MinStockQuantity
            };
            await _context.Articles.AddAsync(article, cancellationToken);
        }
        else
        {
            // Edycja
            var article = await _context.Articles.FindAsync([dto.Id], cancellationToken);
            if (article == null) return (false, "Nie znaleziono artykułu do modyfikacji.");

            article.Code = dto.Code.Trim().ToUpper();
            article.Name = dto.Name.Trim();
            article.Ean = string.IsNullOrWhiteSpace(dto.Ean) ? null : dto.Ean.Trim();
            article.CategoryId = dto.CategoryId;
            article.Unit = dto.Unit;
            article.VatRate = dto.VatRate;
            article.PurchasePriceNet = dto.PurchasePriceNet;
            article.DefaultSalePriceNet = dto.DefaultSalePriceNet;
            article.StockQuantity = dto.StockQuantity;
            article.MinStockQuantity = dto.MinStockQuantity;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // Zabezpieczenie integralności ERP: Sprawdzenie powiązań z zamówieniami
        var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ArticleId == id, cancellationToken);
        if (hasOrders)
        {
            return (false, "Nie można usunąć artykułu, ponieważ występują powiązane zamówienia (dokumenty).");
        }

        var article = await _context.Articles.FindAsync([id], cancellationToken);
        if (article == null) return (false, "Nie odnaleziono towaru.");

        _context.Articles.Remove(article);
        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}