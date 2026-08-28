using MagMini.Application.Common.Models;
using MagMini.Application.DTOs.Orders;
using MagMini.Domain.Enums;

namespace MagMini.Application.Common.Interfaces;

public interface IOrderService
{
    Task<PagedResult<OrderDto>> GetPagedAsync(OrderFilterDto filter, CancellationToken cancellationToken = default);
    Task<SaveOrderDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<string> GenerateNextOrderNumberAsync(CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> SaveAsync(SaveOrderDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> ChangeStatusAsync(int id, OrderStatus newStatus, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}