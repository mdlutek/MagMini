using System.Windows;
using MagMini.UI.ViewModels;

namespace MagMini.UI.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;

        Loaded += async (s, e) =>
        {
            await _viewModel.InitializeAsync();
        };

        _viewModel.LogoutRequested += () =>
        {
            // Zamknięcie głównego okna i wywołanie restartu cyklu logowania
            DialogResult = false;
            Close();
        };
    }
}