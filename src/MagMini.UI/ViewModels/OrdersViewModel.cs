using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Application.Common.Interfaces;
using MagMini.Application.DTOs.Orders;
using MagMini.Domain.Enums;
using MagMini.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace MagMini.UI.ViewModels;

public partial class OrdersViewModel : ObservableObject
{
    private readonly IOrderService _orderService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<OrderDto> _orders = new();

    [ObservableProperty]
    private OrderDto? _selectedOrder;

    [ObservableProperty]
    private string _searchPhrase = string.Empty;

    [ObservableProperty]
    private OrderStatus? _selectedStatus;

    [ObservableProperty]
    private int _pageNumber = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount = 0;

    [ObservableProperty]
    private bool _isLoading;

    public OrdersViewModel(IOrderService orderService, IServiceProvider serviceProvider)
    {
        _orderService = orderService;
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var filter = new OrderFilterDto
            {
                SearchPhrase = SearchPhrase,
                Status = SelectedStatus,
                PageNumber = PageNumber,
                PageSize = 200
            };

            var result = await _orderService.GetPagedAsync(filter);
            Orders = new ObservableCollection<OrderDto>(result.Items);
            TotalCount = result.TotalCount;
            TotalPages = Math.Max(1, result.TotalPages);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        PageNumber = 1;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (PageNumber < TotalPages)
        {
            PageNumber++;
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (PageNumber > 1)
        {
            PageNumber--;
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task AddOrderAsync()
    {
        var dialog = _serviceProvider.GetRequiredService<OrderEditDialog>();
        var vm = (OrderEditViewModel)dialog.DataContext;
        await vm.InitializeAsync(0);

        dialog.Owner = System.Windows.Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task EditOrderAsync()
    {
        if (SelectedOrder == null) return;

        var dialog = _serviceProvider.GetRequiredService<OrderEditDialog>();
        var vm = (OrderEditViewModel)dialog.DataContext;
        await vm.InitializeAsync(SelectedOrder.Id);

        dialog.Owner = System.Windows.Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task ChangeStatusAsync(string statusParam)
    {
        if (SelectedOrder == null || !Enum.TryParse<OrderStatus>(statusParam, out var newStatus)) return;

        var result = await _orderService.ChangeStatusAsync(SelectedOrder.Id, newStatus);
        if (result.Success)
        {
            await LoadDataAsync();
        }
        else
        {
            MessageBox.Show(result.Error, "Błąd zmiany statusu", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private async Task DeleteOrderAsync()
    {
        if (SelectedOrder == null) return;

        var confirm = MessageBox.Show(
            $"Czy na pewno chcesz usunąć zamówienie: {SelectedOrder.OrderNumber}?",
            "Potwierdzenie usunięcia", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm == MessageBoxResult.Yes)
        {
            var result = await _orderService.DeleteAsync(SelectedOrder.Id);
            if (result.Success)
            {
                await LoadDataAsync();
            }
            else
            {
                MessageBox.Show(result.Error, "Błąd operacji", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}