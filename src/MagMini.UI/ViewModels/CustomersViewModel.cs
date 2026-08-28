using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Application.Common.Interfaces;
using MagMini.Application.DTOs.Customers;
using MagMini.Domain.Entities;
using MagMini.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;

namespace MagMini.UI.ViewModels;

public partial class CustomersViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _customers = new();

    [ObservableProperty]
    private CustomerDto? _selectedCustomer;

    [ObservableProperty]
    private string _searchPhrase = string.Empty;

    [ObservableProperty]
    private int _pageNumber = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount = 0;

    [ObservableProperty]
    private bool _isLoading;

    public CustomersViewModel(ICustomerService customerService, IServiceProvider serviceProvider)
    {
        _customerService = customerService;
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
            var filter = new CustomerFilterDto
            {
                SearchPhrase = SearchPhrase,
                PageNumber = PageNumber,
                PageSize = 200
            };

            var result = await _customerService.GetPagedAsync(filter);
            Customers = new ObservableCollection<CustomerDto>(result.Items);
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
    private async Task AddCustomerAsync()
    {
        var dialog = _serviceProvider.GetRequiredService<CustomerEditDialog>();
        var vm = (CustomerEditViewModel)dialog.DataContext;
        await vm.InitializeAsync(0);

        dialog.Owner = System.Windows.Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task EditCustomerAsync()
    {
        if (SelectedCustomer == null) return;

        var dialog = _serviceProvider.GetRequiredService<CustomerEditDialog>();
        var vm = (CustomerEditViewModel)dialog.DataContext;
        await vm.InitializeAsync(SelectedCustomer.Id);

        dialog.Owner = System.Windows.Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteCustomerAsync()
    {
        if (SelectedCustomer == null) return;

        var confirm = MessageBox.Show(
            $"Czy na pewno chcesz usunąć kontrahenta: {SelectedCustomer.Name} ({SelectedCustomer.Code})?",
            "Potwierdzenie usunięcia", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm == MessageBoxResult.Yes)
        {
            var result = await _customerService.DeleteAsync(SelectedCustomer.Id);
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