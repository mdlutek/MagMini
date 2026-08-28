using System.Windows;
using MagMini.Application.Common.Interfaces;
using MagMini.Infrastructure;
using MagMini.Infrastructure.Persistence;
using MagMini.UI.Services;
using MagMini.UI.ViewModels;
using MagMini.UI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MagMini.UI;

public partial class App : System.Windows.Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Rejestracja warstwy danych i serwisów
                services.AddInfrastructure(context.Configuration);

                // Kontekst użytkownika (audyt)
                services.AddSingleton<CurrentUserService>();
                services.AddSingleton<ICurrentUserService>(sp => sp.GetRequiredService<CurrentUserService>());

                // 1. Okno Główne i Logowanie
                services.AddTransient<LoginViewModel>();
                services.AddTransient<LoginView>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainWindow>();

                // 2. Panel Główny
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<DashboardView>();

                // 3. Moduł Artykułów
                services.AddTransient<ArticlesViewModel>();
                services.AddTransient<ArticlesView>();
                services.AddTransient<ArticleEditViewModel>();
                services.AddTransient<ArticleEditDialog>();

                // 4. Moduł Kontrahentów
                services.AddTransient<CustomersViewModel>();
                services.AddTransient<CustomersView>();
                services.AddTransient<CustomerEditViewModel>();
                services.AddTransient<CustomerEditDialog>();

                // 5. Moduł Zamówień (ZK)
                services.AddTransient<OrdersViewModel>();
                services.AddTransient<OrdersView>();
                services.AddTransient<OrderEditViewModel>();
                services.AddTransient<OrderEditDialog>();

                // 6. Moduł Kategorii
                services.AddTransient<CategoriesViewModel>();
                services.AddTransient<CategoriesView>();
                services.AddTransient<CategoryEditViewModel>();
                services.AddTransient<CategoryEditDialog>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var splash = new SplashScreenView();
        splash.Show();

        try
        {
            await Task.Run(async () =>
            {
                splash.UpdateStatus("Uruchamianie usług systemowych...");
                await AppHost!.StartAsync();

                splash.UpdateStatus("Sprawdzanie i migracja bazy danych...");
                using (var scope = AppHost.Services.CreateScope())
                {
                    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
                    await dbInitializer.InitializeAsync();
                }

                splash.UpdateStatus("Gotowe!");
                await Task.Delay(300);
            });

            splash.Close();

            // Pętla sesji użytkownika (umożliwia wylogowanie i ponowne zalogowanie)
            RunApplicationLoop();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd podczas inicjalizacji aplikacji:\n{ex.Message}", "Błąd krytyczny", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private void RunApplicationLoop()
    {
        while (true)
        {
            var loginView = AppHost!.Services.GetRequiredService<LoginView>();
            var loginResult = loginView.ShowDialog();

            if (loginResult != true)
            {
                // Użytkownik zamknął okno logowania -> wyjście z programu
                Shutdown();
                break;
            }

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.ShowDialog(); // Otwieramy jako dialog sesji roboczej
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (AppHost != null)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
        }
        base.OnExit(e);
    }
}