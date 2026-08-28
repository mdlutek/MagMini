using MagMini.Mobile.ViewModels;

namespace MagMini.Mobile.Views;

public partial class ScannerPage : ContentPage
{
    public ScannerPage(ScannerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}