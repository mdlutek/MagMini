using System.Net.Http.Json;
using System.Text.Json;
using MagMini.Application.Common.Models;
using MagMini.Application.DTOs.Orders;
using MagMini.Domain.Enums;

namespace MagMini.Mobile.Services;

public class OrderMobileService
{
    private readonly HttpClient _httpClient;

    public OrderMobileService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<OrderDto>> GetOrdersAsync(string? search = null)
    {
        try
        {
            var url = string.IsNullOrWhiteSpace(search)
                ? "api/orders?PageSize=100"
                : $"api/orders?SearchPhrase={Uri.EscapeDataString(search)}&PageSize=100";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<OrderDto>();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<PagedResult<OrderDto>>(json, options);

            return result?.Items.ToList() ?? new List<OrderDto>();
        }
        catch
        {
            return new List<OrderDto>();
        }
    }

    public async Task<SaveOrderDto?> GetOrderDetailsAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SaveOrderDto>($"api/orders/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Success, string? Error)> ChangeStatusAsync(int id, OrderStatus newStatus)
    {
        try
        {
            var response = await _httpClient.PutAsync($"api/orders/{id}/status?status={newStatus}", null);
            if (response.IsSuccessStatusCode) return (true, null);

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}