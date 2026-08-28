using System.Net.Http.Json;
using System.Text.Json;
using MagMini.Application.DTOs.Auth;

namespace MagMini.Mobile.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool Success, string? Error)> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { Username = username, Password = password });
            if (!response.IsSuccessStatusCode)
            {
                return (false, "Niepoprawny login lub hasło.");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("token").GetString();

            if (string.IsNullOrEmpty(token)) return (false, "Niepoprawna odpowiedź serwera.");

            // Zapis w bezpiecznej pamięci telefonu
            await SecureStorage.Default.SetAsync("jwt_token", token);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Błąd połączenia z serwerem: {ex.Message}");
        }
    }

    public void Logout()
    {
        SecureStorage.Default.Remove("jwt_token");
    }
}