using System.Windows;
using MagMini.UI.ViewModels;

namespace MagMini.UI.Views;

public partial class LoginView : Window
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.LoginSuccessful += () =>
        {
            DialogResult = true;
            Close();
        };

        Loaded += (s, e) =>
        {
            // Domyślny fokus na hasło (ponieważ login jest już wpisany domyślnie)
            UserPasswordBox.Focus();
        };
    }
}