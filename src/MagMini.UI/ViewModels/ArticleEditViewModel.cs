using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Application.Common.Interfaces;
using MagMini.Application.DTOs.Articles;
using MagMini.Domain.Entities;
using MagMini.Domain.Enums;

namespace MagMini.UI.ViewModels;

public partial class ArticleEditViewModel : ObservableObject
{
    private readonly IArticleService _articleService;

    [ObservableProperty]
    private SaveArticleDto _article = new();

    [ObservableProperty]
    private List<Category> _categories = new();

    [ObservableProperty]
    private Array _units = Enum.GetValues(typeof(UnitOfMeasure));

    [ObservableProperty]
    private Array _vatRates = Enum.GetValues(typeof(VatRate));

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _windowTitle = "Nowy Artykuł";

    public event Action? SavedSuccessfully;

    public ArticleEditViewModel(IArticleService articleService)
    {
        _articleService = articleService;
    }

    public async Task InitializeAsync(int articleId = 0)
    {
        Categories = await _articleService.GetCategoriesLookupAsync();

        if (articleId > 0)
        {
            WindowTitle = "Edycja Artykułu";
            var existing = await _articleService.GetForEditAsync(articleId);
            if (existing != null) Article = existing;
        }
        else
        {
            WindowTitle = "Nowy Artykuł";
            Article = new SaveArticleDto
            {
                CategoryId = Categories.FirstOrDefault()?.Id ?? 0,
                Unit = UnitOfMeasure.Pcs,
                VatRate = VatRate.Vat23
            };
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Article.Code) || string.IsNullOrWhiteSpace(Article.Name))
        {
            ErrorMessage = "Kod i Nazwa są wymagane!";
            return;
        }

        var result = await _articleService.SaveAsync(Article);
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