using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Application.Common.Interfaces;
using MagMini.UI.Services;

namespace MagMini.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly CurrentUserService _currentUserService;

    [ObservableProperty]
    private string _username = "admin"; // Domyślnie podpowiadamy admina

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    // Zdarzenie informujące okno o sukcesie logowania (aby zamknąć Dialog z DialogResult = true)
    public event Action? LoginSuccessful;

    public LoginViewModel(IAuthService _authService, CurrentUserService currentUserService)
    {
        this._authService = _authService;
        _currentUserService = currentUserService;
    }

    [RelayCommand]
    private async Task LoginAsync(PasswordBox? passwordBox)
    {
        if (passwordBox == null) return;

        ErrorMessage = null;
        IsLoading = true;

        try
        {
            var result = await _authService.LoginAsync(Username, passwordBox.Password);

            if (result.Success && result.User != null)
            {
                // Ustawienie tożsamości w kontekście sesji
                _currentUserService.SetUser(result.User.Id, result.User.Username);
                LoginSuccessful?.Invoke();
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Błąd logowania.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Błąd połączenia: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}