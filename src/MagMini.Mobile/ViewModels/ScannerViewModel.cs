using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Application.DTOs.Articles;
using MagMini.Mobile.Services;

namespace MagMini.Mobile.ViewModels;

public partial class ScannerViewModel : ObservableObject
{
    private readonly ArticleMobileService _articleService;

    [ObservableProperty]
    private string _barcodeInput = string.Empty;

    [ObservableProperty]
    private ArticleDto? _scannedArticle;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private string? _statusMessage;

    public ScannerViewModel(ArticleMobileService articleService)
    {
        _articleService = articleService;
    }

    [RelayCommand]
    public async Task SearchBarcodeAsync(string? code = null)
    {
        var targetCode = code ?? BarcodeInput;
        if (string.IsNullOrWhiteSpace(targetCode)) return;

        IsSearching = true;
        StatusMessage = "Wyszukiwanie w bazie...";
        ScannedArticle = null;

        try
        {
            var (items, error) = await _articleService.GetArticlesAsync(targetCode.Trim());

            // Szukamy idealnego dopasowania po EAN lub Symbolu (SKU)
            var match = items.FirstOrDefault(a =>
                string.Equals(a.Ean, targetCode.Trim(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(a.Code, targetCode.Trim(), StringComparison.OrdinalIgnoreCase))
                ?? items.FirstOrDefault();

            if (match != null)
            {
                ScannedArticle = match;
                StatusMessage = "✅ Znaleziono artykuł!";
            }
            else
            {
                StatusMessage = $"❌ Nie znaleziono artykułu dla kodu: {targetCode}";
            }
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        BarcodeInput = string.Empty;
        ScannedArticle = null;
        StatusMessage = null;
    }
}