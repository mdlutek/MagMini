using MagMini.Application.Common.Interfaces;
using MagMini.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MagMini.Infrastructure.Persistence;

public class DbInitializer
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DbInitializer(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task InitializeAsync()
    {
        // Wykonanie brakujących migracji
        if ((await _context.Database.GetPendingMigrationsAsync()).Any())
        {
            await _context.Database.MigrateAsync();
        }

        await SeedDefaultDataAsync();
    }

    private async Task SeedDefaultDataAsync()
    {
        if (!await _context.Roles.AnyAsync())
        {
            var adminRole = new Role { Name = "Administrator", Description = "Pełny dostęp do systemu" };
            var userRole = new Role { Name = "Magazynier", Description = "Dostęp operacyjny" };

            await _context.Roles.AddRangeAsync(adminRole, userRole);
            await _context.SaveChangesAsync();

            // Domyślny użytkownik admin / admin123
            var adminUser = new User
            {
                Username = "admin",
                FullName = "Administrator Systemu",
                PasswordHash = _passwordHasher.HashPassword("admin123"),
                RoleId = adminRole.Id,
                IsActive = true
            };
            await _context.Users.AddAsync(adminUser);
            await _context.SaveChangesAsync();
        }

        if (!await _context.Categories.AnyAsync())
        {
            await _context.Categories.AddRangeAsync(
                new Category { Code = "AGD", Name = "Sprzęt AGD", Description = "Artykuły gospodarstwa domowego" },
                new Category { Code = "RTV", Name = "Sprzęt RTV", Description = "Elektronika użytkowa" },
                new Category { Code = "BIURO", Name = "Materiały Biurowe", Description = "Papier, tonery, itp." }
            );
            await _context.SaveChangesAsync();
        }
    }
}