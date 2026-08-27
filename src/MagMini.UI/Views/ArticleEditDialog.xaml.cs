using System.Windows;
using MagMini.UI.ViewModels;

namespace MagMini.UI.Views;

public partial class ArticleEditDialog : Window
{
    public ArticleEditDialog(ArticleEditViewModel viewModel)
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