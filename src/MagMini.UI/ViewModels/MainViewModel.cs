using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Application.Common.Interfaces;
using MagMini.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MagMini.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserService _currentUserService;

    [ObservableProperty]
    private ObservableObject? _currentView;

    [ObservableProperty]
    private string _currentUserName = string.Empty;

    [ObservableProperty]
    private string _databaseName = "MagMiniDb";

    [ObservableProperty]
    private string _serverName = "localhost (.)";

    public event Action? LogoutRequested;

    public MainViewModel(IServiceProvider serviceProvider, CurrentUserService currentUserService)
    {
        _serviceProvider = serviceProvider;
        _currentUserService = currentUserService;
    }

    public async Task InitializeAsync()
    {
        CurrentUserName = _currentUserService.Username ?? "Nieznany";
        await NavigateToDashboardAsync();
    }

    [RelayCommand]
    private async Task NavigateToDashboardAsync()
    {
        var dashboardVm = _serviceProvider.GetRequiredService<DashboardViewModel>();
        await dashboardVm.LoadStatisticsAsync();
        CurrentView = dashboardVm;
    }

    [RelayCommand]
    private async Task NavigateToArticlesAsync()
    {
        var articlesVm = _serviceProvider.GetRequiredService<ArticlesViewModel>();
        await articlesVm.InitializeAsync();
        CurrentView = articlesVm;
    }

    [RelayCommand]
    private async Task NavigateToCategoriesAsync()
    {
        var categoriesVm = _serviceProvider.GetRequiredService<CategoriesViewModel>();
        await categoriesVm.InitializeAsync();
        CurrentView = categoriesVm;
    }

    [RelayCommand]
    private async Task NavigateToCustomersAsync()
    {
        var customersVm = _serviceProvider.GetRequiredService<CustomersViewModel>();
        await customersVm.InitializeAsync();
        CurrentView = customersVm;
    }

    [RelayCommand]
    private async Task NavigateToOrdersAsync()
    {
        var ordersVm = _serviceProvider.GetRequiredService<OrdersViewModel>();
        await ordersVm.InitializeAsync();
        CurrentView = ordersVm;
    }

    [RelayCommand]
    private void Logout()
    {
        _currentUserService.Clear();
        LogoutRequested?.Invoke();
    }
}