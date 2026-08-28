using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Application.DTOs.Articles;
using MagMini.Mobile.Services;

namespace MagMini.Mobile.ViewModels;

public partial class ArticlesViewModel : ObservableObject
{
    private readonly ArticleMobileService _articleService;
    private readonly AuthService _authService;

    [ObservableProperty]
    private ObservableCollection<ArticleDto> _articles = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string? _errorMessage;

    public ArticlesViewModel(ArticleMobileService articleService, AuthService authService)
    {
        _articleService = articleService;
        _authService = authService;
    }

    [RelayCommand]
    public async Task LoadArticlesAsync()
    {
        ErrorMessage = null;
        IsRefreshing = true;

        try
        {
            var (items, error) = await _articleService.GetArticlesAsync(SearchText);

            if (!string.IsNullOrEmpty(error))
            {
                ErrorMessage = error;
            }
            else
            {
                Articles = new ObservableCollection<ArticleDto>(items);
                if (!Articles.Any())
                {
                    ErrorMessage = "Baza jest pusta lub brak artykułów spełniających kryteria.";
                }
            }
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadArticlesAsync();
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        _authService.Logout();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}