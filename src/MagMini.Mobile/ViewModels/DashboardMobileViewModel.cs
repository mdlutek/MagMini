using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Mobile.Services;

namespace MagMini.Mobile.ViewModels;

public partial class DashboardMobileViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private string _username = "admin";

    public DashboardMobileViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task NavigateToArticlesAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.ArticlesPage));
    }

    [RelayCommand]
    private async Task NavigateToOrdersAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.OrdersPage));
    }

    [RelayCommand]
    private async Task NavigateToScannerAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.ScannerPage));
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        _authService.Logout();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}