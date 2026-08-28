using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Application.DTOs.Orders;
using MagMini.Domain.Enums;
using MagMini.Mobile.Services;

namespace MagMini.Mobile.ViewModels;

[QueryProperty(nameof(OrderId), "OrderId")]
public partial class OrderDetailViewModel : ObservableObject
{
    private readonly OrderMobileService _orderService;

    [ObservableProperty]
    private int _orderId;

    [ObservableProperty]
    private SaveOrderDto? _order;

    [ObservableProperty]
    private ObservableCollection<OrderItemDto> _items = new();

    [ObservableProperty]
    private bool _isLoading;

    // Przycisk realizacji dostępny tylko dla zamówień niezrealizowanych i nieanulowanych
    [ObservableProperty]
    private bool _canComplete;

    public OrderDetailViewModel(OrderMobileService orderService)
    {
        _orderService = orderService;
    }

    async partial void OnOrderIdChanged(int value)
    {
        if (value > 0)
        {
            await LoadOrderDetailsAsync(value);
        }
    }

    public async Task LoadOrderDetailsAsync(int id)
    {
        IsLoading = true;
        try
        {
            var details = await _orderService.GetOrderDetailsAsync(id);
            if (details != null)
            {
                Order = details;
                Items = new ObservableCollection<OrderItemDto>(details.Items);

                // Blokada dla dokumentów zrealizowanych / anulowanych
                CanComplete = details.Status != OrderStatus.Completed && details.Status != OrderStatus.Cancelled;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CompleteOrderAsync()
    {
        if (Order == null || !CanComplete) return;

        var confirm = await Shell.Current.DisplayAlertAsync(
            "Potwierdzenie wydania",
            $"Czy na pewno chcesz zrealizować i wydać z magazynu zamówienie {Order.OrderNumber}?",
            "Tak, wydaj towar", "Anuluj");

        if (confirm)
        {
            var result = await _orderService.ChangeStatusAsync(Order.Id, OrderStatus.Completed);
            if (result.Success)
            {
                await Shell.Current.DisplayAlertAsync("Sukces", "Zamówienie zrealizowane. Stany magazynowe zaktualizowane!", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Błąd", result.Error ?? "Błąd zmiany statusu", "OK");
            }
        }
    }
}