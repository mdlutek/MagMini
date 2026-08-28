using MagMini.Mobile.ViewModels;

namespace MagMini.Mobile.Views;

public partial class ArticlesPage : ContentPage
{
    private readonly ArticlesViewModel _viewModel;

    public ArticlesPage(ArticlesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadArticlesAsync();
    }
}