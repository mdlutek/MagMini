using MagMini.Application.Common.Models;
using MagMini.Application.DTOs.Customers;

namespace MagMini.Application.Common.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetPagedAsync(CustomerFilterDto filter, CancellationToken cancellationToken = default);
    Task<SaveCustomerDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> SaveAsync(SaveCustomerDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}