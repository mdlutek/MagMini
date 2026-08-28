using MagMini.Application.Common.Interfaces;
using MagMini.Application.Common.Models;
using MagMini.Application.DTOs.Orders;
using MagMini.Domain.Entities;
using MagMini.Domain.Enums;
using MagMini.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MagMini.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<OrderDto>> GetPagedAsync(OrderFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchPhrase))
        {
            var phrase = filter.SearchPhrase.Trim().ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(phrase) ||
                o.Customer.Name.ToLower().Contains(phrase) ||
                o.Customer.Code.ToLower().Contains(phrase) ||
                (o.Customer.Nip != null && o.Customer.Nip.Contains(phrase)));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(o => o.Status == filter.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer.Name,
                CustomerCode = o.Customer.Code,
                CustomerNip = o.Customer.Nip,
                Status = o.Status,
                TotalNet = o.Items.Sum(i => i.Quantity * i.UnitPriceNet),
                TotalGross = o.Items.Sum(i => (i.Quantity * i.UnitPriceNet) * (1 + (i.VatRate > 0 ? (decimal)i.VatRate / 100m : 0m))),
                ItemsCount = o.Items.Count,
                CreatedBy = o.CreatedBy
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<SaveOrderDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Article)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null) return null;

        return new SaveOrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            CustomerId = order.CustomerId,
            Status = order.Status,
            Remarks = order.Remarks,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ArticleId = i.ArticleId,
                ArticleCode = i.Article.Code,
                ArticleName = i.Article.Name,
                Quantity = i.Quantity,
                UnitPriceNet = i.UnitPriceNet,
                VatRate = i.VatRate
            }).ToList()
        };
    }

    public async Task<string> GenerateNextOrderNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        var prefix = $"ZK/{year}/{month:D2}/";

        var lastOrderNumber = await _context.Orders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int nextSeq = 1;
        if (!string.IsNullOrEmpty(lastOrderNumber) && lastOrderNumber.Length >= prefix.Length + 4)
        {
            var seqStr = lastOrderNumber.Substring(prefix.Length);
            if (int.TryParse(seqStr, out int currentSeq))
            {
                nextSeq = currentSeq + 1;
            }
        }

        return $"{prefix}{nextSeq:D4}";
    }

    public async Task<(bool Success, string? Error)> SaveAsync(SaveOrderDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.CustomerId <= 0)
            return (false, "Wybierz kontrahenta dla zamówienia.");

        if (!dto.Items.Any())
            return (false, "Zamówienie musi zawierać co najmniej jeden artykuł.");

        if (dto.Id == 0)
        {
            var order = new Order
            {
                OrderNumber = string.IsNullOrWhiteSpace(dto.OrderNumber) ? await GenerateNextOrderNumberAsync(cancellationToken) : dto.OrderNumber,
                OrderDate = dto.OrderDate,
                CustomerId = dto.CustomerId,
                Status = dto.Status,
                Remarks = dto.Remarks,
                Items = dto.Items.Select(i => new OrderItem
                {
                    ArticleId = i.ArticleId,
                    Quantity = i.Quantity,
                    UnitPriceNet = i.UnitPriceNet,
                    VatRate = i.VatRate
                }).ToList()
            };

            await _context.Orders.AddAsync(order, cancellationToken);
        }
        else
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == dto.Id, cancellationToken);

            if (order == null) return (false, "Nie odnaleziono zamówienia.");

            if (order.Status == OrderStatus.Completed)
                return (false, "Zrealizowane zamówienie nie może być modyfikowane.");

            order.OrderDate = dto.OrderDate;
            order.CustomerId = dto.CustomerId;
            order.Remarks = dto.Remarks;

            // Aktualizacja pozycji
            _context.OrderItems.RemoveRange(order.Items);
            order.Items = dto.Items.Select(i => new OrderItem
            {
                OrderId = order.Id,
                ArticleId = i.ArticleId,
                Quantity = i.Quantity,
                UnitPriceNet = i.UnitPriceNet,
                VatRate = i.VatRate
            }).ToList();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangeStatusAsync(int id, OrderStatus newStatus, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null) return (false, "Nie znaleziono dokumentu.");

        if (order.Status == newStatus) return (true, null);

        // Zmiana na Zrealizowane (Wydanie z magazynu)
        if (newStatus == OrderStatus.Completed && order.Status != OrderStatus.Completed)
        {
            foreach (var item in order.Items)
            {
                var article = await _context.Articles.FindAsync([item.ArticleId], cancellationToken);
                if (article != null)
                {
                    article.StockQuantity -= item.Quantity; // Zmniejszenie stanu magazynowego
                }
            }
        }

        // Anulowanie zamówienia, które wcześniej zdjęło stan
        if (newStatus == OrderStatus.Cancelled && order.Status == OrderStatus.Completed)
        {
            foreach (var item in order.Items)
            {
                var article = await _context.Articles.FindAsync([item.ArticleId], cancellationToken);
                if (article != null)
                {
                    article.StockQuantity += item.Quantity; // Przywrócenie stanu
                }
            }
        }

        order.Status = newStatus;
        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync([id], cancellationToken);
        if (order == null) return (false, "Nie znaleziono zamówienia.");

        if (order.Status == OrderStatus.Completed)
            return (false, "Nie można usunąć zrealizowanego zamówienia.");

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}