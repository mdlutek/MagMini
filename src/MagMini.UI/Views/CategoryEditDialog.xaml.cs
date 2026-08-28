using System.Windows;
using MagMini.UI.ViewModels;

namespace MagMini.UI.Views;

public partial class CategoryEditDialog : Window
{
    public CategoryEditDialog(CategoryEditViewModel viewModel)
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