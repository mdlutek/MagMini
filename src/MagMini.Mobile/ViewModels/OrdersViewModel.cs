using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Application.DTOs.Orders;
using MagMini.Domain.Entities;
using MagMini.Mobile.Services;
using MagMini.Mobile.Views;
using System.Collections.ObjectModel;

namespace MagMini.Mobile.ViewModels;

public partial class OrdersViewModel : ObservableObject
{
    private readonly OrderMobileService _orderService;

    [ObservableProperty]
    private ObservableCollection<OrderDto> _orders = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isRefreshing;

    public OrdersViewModel(OrderMobileService orderService)
    {
        _orderService = orderService;
    }

    [RelayCommand]
    public async Task LoadOrdersAsync()
    {
        IsRefreshing = true;
        try
        {
            var items = await _orderService.GetOrdersAsync(SearchText);
            Orders = new ObservableCollection<OrderDto>(items);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task OpenOrderDetailsAsync(OrderDto order)
    {
        if (order == null) return;

        await Shell.Current.GoToAsync(nameof(OrderDetailPage), new Dictionary<string, object>
        {
            { "OrderId", order.Id }
        });
    }
}