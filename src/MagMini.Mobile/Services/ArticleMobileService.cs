using System.Text.Json;
using MagMini.Application.Common.Models;
using MagMini.Application.DTOs.Articles;

namespace MagMini.Mobile.Services;

public class ArticleMobileService
{
    private readonly HttpClient _httpClient;

    public ArticleMobileService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(List<ArticleDto> Items, string? Error)> GetArticlesAsync(string? search = null)
    {
        try
        {
            var url = string.IsNullOrWhiteSpace(search)
                ? "api/articles?PageSize=100"
                : $"api/articles?SearchPhrase={Uri.EscapeDataString(search)}&PageSize=100";

            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (new List<ArticleDto>(), "Błąd 401: Brak autoryzacji (Token wygasł lub nie został przesłany).");
            }

            if (!response.IsSuccessStatusCode)
            {
                return (new List<ArticleDto>(), $"Błąd serwera: {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<PagedResult<ArticleDto>>(json, options);

            return (result?.Items?.ToList() ?? new List<ArticleDto>(), null);
        }
        catch (Exception ex)
        {
            return (new List<ArticleDto>(), $"Błąd połączenia: {ex.Message}");
        }
    }
}