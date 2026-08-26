using System.Windows;

namespace MagMini.UI.Views;

public partial class SplashScreenView : Window
{
    public SplashScreenView()
    {
        InitializeComponent();
    }

    public void UpdateStatus(string message)
    {
        Dispatcher.Invoke(() =>
        {
            StatusTextBlock.Text = message;
        });
    }
}