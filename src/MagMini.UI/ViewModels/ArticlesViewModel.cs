using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Application.Common.Interfaces;
using MagMini.Application.DTOs.Articles;
using MagMini.Domain.Entities;
using MagMini.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace MagMini.UI.ViewModels;

public partial class ArticlesViewModel : ObservableObject
{
    private readonly IArticleService _articleService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<ArticleDto> _articles = new();

    [ObservableProperty]
    private ArticleDto? _selectedArticle;

    [ObservableProperty]
    private List<Category> _categories = new();

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private string _searchPhrase = string.Empty;

    // Paginacja (sztywno co 200)
    [ObservableProperty]
    private int _pageNumber = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount = 0;

    [ObservableProperty]
    private bool _isLoading;

    public ArticlesViewModel(IArticleService articleService, IServiceProvider serviceProvider)
    {
        _articleService = articleService;
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeAsync()
    {
        var cats = await _articleService.GetCategoriesLookupAsync();
        // Opcja "Wszystkie kategorie"
        var allCats = new List<Category> { new() { Id = 0, Name = "-- Wszystkie kategorie --" } };
        allCats.AddRange(cats);
        Categories = allCats;
        SelectedCategory = Categories.First();

        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var filter = new ArticleFilterDto
            {
                SearchPhrase = SearchPhrase,
                CategoryId = SelectedCategory?.Id,
                PageNumber = PageNumber,
                PageSize = 200 // Wymóg: 200 elementów
            };

            var result = await _articleService.GetPagedAsync(filter);

            Articles = new ObservableCollection<ArticleDto>(result.Items);
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
    private async Task AddArticleAsync()
    {
        var dialog = _serviceProvider.GetRequiredService<ArticleEditDialog>();
        var vm = (ArticleEditViewModel)dialog.DataContext;
        await vm.InitializeAsync(0);

        dialog.Owner = System.Windows.Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task EditArticleAsync()
    {
        if (SelectedArticle == null) return;

        var dialog = _serviceProvider.GetRequiredService<ArticleEditDialog>();
        var vm = (ArticleEditViewModel)dialog.DataContext;
        await vm.InitializeAsync(SelectedArticle.Id);

        dialog.Owner = System.Windows.Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteArticleAsync()
    {
        if (SelectedArticle == null) return;

        var confirm = MessageBox.Show(
            $"Czy na pewno chcesz usunąć artykuł: {SelectedArticle.Name} ({SelectedArticle.Code})?",
            "Potwierdzenie usunięcia", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm == MessageBoxResult.Yes)
        {
            var result = await _articleService.DeleteAsync(SelectedArticle.Id);
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