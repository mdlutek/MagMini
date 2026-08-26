namespace MagMini.Application.DTOs.Auth;

public class LoginResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public UserDto? User { get; set; }

    public static LoginResult Succeeded(UserDto user) => new() { Success = true, User = user };
    public static LoginResult Failed(string message) => new() { Success = false, ErrorMessage = message };
}