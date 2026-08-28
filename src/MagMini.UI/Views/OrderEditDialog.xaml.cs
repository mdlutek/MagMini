using System.Windows;
using MagMini.UI.ViewModels;

namespace MagMini.UI.Views;

public partial class OrderEditDialog : Window
{
    public OrderEditDialog(OrderEditViewModel viewModel)
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