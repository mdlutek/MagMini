using MagMini.Application.DTOs.Auth;

namespace MagMini.Application.Common.Interfaces;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
}