using MagMini.Application.Common.Interfaces;
using MagMini.Application.Common.Models;
using MagMini.Application.Common.Validators;
using MagMini.Application.DTOs.Customers;
using MagMini.Domain.Entities;
using MagMini.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MagMini.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CustomerDto>> GetPagedAsync(CustomerFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers
            .AsNoTracking()
            .AsQueryable();

        // 1. Filtrowanie globalne po frazie (Symbol, Nazwa, NIP, Miasto)
        if (!string.IsNullOrWhiteSpace(filter.SearchPhrase))
        {
            var phrase = filter.SearchPhrase.Trim().ToLower();
            query = query.Where(c =>
                c.Code.ToLower().Contains(phrase) ||
                c.Name.ToLower().Contains(phrase) ||
                (c.Nip != null && c.Nip.Contains(phrase)) ||
                (c.City != null && c.City.ToLower().Contains(phrase)));
        }

        // 2. Filtrowanie po typie (Firmy / Osoby fizyczne)
        if (filter.IsCompanyOnly.HasValue)
        {
            query = query.Where(c => c.IsCompany == filter.IsCompanyOnly.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Code)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Nip = c.Nip,
                IsCompany = c.IsCompany,
                City = c.City,
                PostalCode = c.PostalCode,
                Street = c.Street,
                Phone = c.Phone,
                Email = c.Email
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<CustomerDto>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<SaveCustomerDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FindAsync([id], cancellationToken);
        if (customer == null) return null;

        return new SaveCustomerDto
        {
            Id = customer.Id,
            Code = customer.Code,
            Name = customer.Name,
            Nip = customer.Nip,
            IsCompany = customer.IsCompany,
            Street = customer.Street,
            PostalCode = customer.PostalCode,
            City = customer.City,
            Phone = customer.Phone,
            Email = customer.Email
        };
    }

    public async Task<(bool Success, string? Error)> SaveAsync(SaveCustomerDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Walidacja unikalności Symbolu kontrahenta
        var codeExists = await _context.Customers
            .AnyAsync(c => c.Code.ToLower() == dto.Code.Trim().ToLower() && c.Id != dto.Id, cancellationToken);

        if (codeExists)
            return (false, $"Kontrahent o symbolu '{dto.Code}' już istnieje.");

        // 2. Walidacja NIP dla firm
        if (dto.IsCompany)
        {
            if (string.IsNullOrWhiteSpace(dto.Nip))
                return (false, "Dla firm numer NIP jest wymagany.");

            if (!NipValidator.IsValid(dto.Nip))
                return (false, "Podany numer NIP jest nieprawidłowy (błędna suma kontrolna).");
        }

        if (dto.Id == 0)
        {
            var customer = new Customer
            {
                Code = dto.Code.Trim().ToUpper(),
                Name = dto.Name.Trim(),
                Nip = dto.IsCompany ? dto.Nip?.Replace("-", "").Replace(" ", "").Trim() : null,
                IsCompany = dto.IsCompany,
                Street = dto.Street?.Trim(),
                PostalCode = dto.PostalCode?.Trim(),
                City = dto.City?.Trim(),
                Phone = dto.Phone?.Trim(),
                Email = dto.Email?.Trim()
            };
            await _context.Customers.AddAsync(customer, cancellationToken);
        }
        else
        {
            var customer = await _context.Customers.FindAsync([dto.Id], cancellationToken);
            if (customer == null) return (false, "Nie odnaleziono kontrahenta.");

            customer.Code = dto.Code.Trim().ToUpper();
            customer.Name = dto.Name.Trim();
            customer.Nip = dto.IsCompany ? dto.Nip?.Replace("-", "").Replace(" ", "").Trim() : null;
            customer.IsCompany = dto.IsCompany;
            customer.Street = dto.Street?.Trim();
            customer.PostalCode = dto.PostalCode?.Trim();
            customer.City = dto.City?.Trim();
            customer.Phone = dto.Phone?.Trim();
            customer.Email = dto.Email?.Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // Blokada usunięcia kontrahenta z zamówieniami
        var hasOrders = await _context.Orders.AnyAsync(o => o.CustomerId == id, cancellationToken);
        if (hasOrders)
            return (false, "Nie można usunąć kontrahenta, ponieważ są do niego przypisane zamówienia.");

        var customer = await _context.Customers.FindAsync([id], cancellationToken);
        if (customer == null) return (false, "Nie odnaleziono kontrahenta.");

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}