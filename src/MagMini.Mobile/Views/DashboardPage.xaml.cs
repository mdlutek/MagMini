using MagMini.Mobile.ViewModels;

namespace MagMini.Mobile.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardMobileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}