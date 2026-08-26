using MagMini.Application.Common.Interfaces;
using MagMini.Application.DTOs.Auth;
using MagMini.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MagMini.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return LoginResult.Failed("Wprowadź login i hasło.");
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.Trim().ToLower(), cancellationToken);

        if (user == null)
        {
            return LoginResult.Failed("Niepoprawny login lub hasło.");
        }

        if (!user.IsActive)
        {
            return LoginResult.Failed("Konto użytkownika jest zablokowane.");
        }

        bool isPasswordValid = _passwordHasher.VerifyPassword(password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return LoginResult.Failed("Niepoprawny login lub hasło.");
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            RoleName = user.Role.Name,
            IsActive = user.IsActive
        };

        return LoginResult.Succeeded(userDto);
    }
}