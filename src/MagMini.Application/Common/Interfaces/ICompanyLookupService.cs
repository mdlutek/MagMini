using MagMini.Application.DTOs.Customers;

namespace MagMini.Application.Common.Interfaces;

public interface ICompanyLookupService
{
    Task<CompanyLookupResultDto> LookupByNipAsync(string nip, CancellationToken cancellationToken = default);
}