using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Application.Common.Interfaces;
using MagMini.Application.DTOs.Categories;
using MagMini.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace MagMini.UI.ViewModels;

public partial class CategoriesViewModel : ObservableObject
{
    private readonly ICategoryService _categoryService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<CategoryDto> _categories = new();

    [ObservableProperty]
    private CategoryDto? _selectedCategory;

    [ObservableProperty]
    private string _searchPhrase = string.Empty;

    public CategoriesViewModel(ICategoryService categoryService, IServiceProvider serviceProvider)
    {
        _categoryService = categoryService;
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        var list = await _categoryService.GetAllAsync(SearchPhrase);
        Categories = new ObservableCollection<CategoryDto>(list);
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        var dialog = _serviceProvider.GetRequiredService<CategoryEditDialog>();
        var vm = (CategoryEditViewModel)dialog.DataContext;
        await vm.InitializeAsync(0);

        dialog.Owner = System.Windows.Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task EditCategoryAsync()
    {
        if (SelectedCategory == null) return;

        var dialog = _serviceProvider.GetRequiredService<CategoryEditDialog>();
        var vm = (CategoryEditViewModel)dialog.DataContext;
        await vm.InitializeAsync(SelectedCategory.Id);

        dialog.Owner = System.Windows.Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync()
    {
        if (SelectedCategory == null) return;

        var confirm = MessageBox.Show(
            $"Czy na pewno chcesz usunąć kategorię: {SelectedCategory.Name} ({SelectedCategory.Code})?",
            "Potwierdzenie usunięcia", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm == MessageBoxResult.Yes)
        {
            var result = await _categoryService.DeleteAsync(SelectedCategory.Id);
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