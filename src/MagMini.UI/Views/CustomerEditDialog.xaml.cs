using System.Windows;
using MagMini.UI.ViewModels;

namespace MagMini.UI.Views;

public partial class CustomerEditDialog : Window
{
    public CustomerEditDialog(CustomerEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.SavedSuccessfully += () =>
        {
            DialogResult = true;
            Close();
        };
    }
}