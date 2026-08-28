using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Application.Common.Interfaces;
using MagMini.Application.DTOs.Categories;
using MagMini.Domain.Entities;

namespace MagMini.UI.ViewModels;

public partial class CategoryEditViewModel : ObservableObject
{
    private readonly ICategoryService _categoryService;

    [ObservableProperty]
    private SaveCategoryDto _category = new();

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _windowTitle = "Nowa Kategoria";

    public event Action? SavedSuccessfully;

    public CategoryEditViewModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task InitializeAsync(int categoryId = 0)
    {
        ErrorMessage = null;
        if (categoryId > 0)
        {
            WindowTitle = "Edycja Kategorii";
            var existing = await _categoryService.GetForEditAsync(categoryId);
            if (existing != null) Category = existing;
        }
        else
        {
            WindowTitle = "Nowa Kategoria";
            Category = new SaveCategoryDto();
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;
        var result = await _categoryService.SaveAsync(Category);
        if (result.Success)
        {
            SavedSuccessfully?.Invoke();
        }
        else
        {
            ErrorMessage = result.Error;
        }
    }
}