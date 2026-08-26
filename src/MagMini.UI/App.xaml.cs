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
                services.AddInfrastructure(context.Configuration);
                services.AddSingleton<CurrentUserService>();
                services.AddSingleton<ICurrentUserService>(sp => sp.GetRequiredService<CurrentUserService>());

                // Rejestracja View i ViewModeli
                services.AddTransient<LoginViewModel>();
                services.AddTransient<LoginView>();
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Zapobiega zamknięciu aplikacji po zamknięciu SplashScreena
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

            // Zamykamy splash screen dopiero po zakończeniu inicjalizacji
            splash.Close();

            // Pokazujemy okno logowania
            var loginView = AppHost!.Services.GetRequiredService<LoginView>();
            var loginResult = loginView.ShowDialog();

            if (loginResult == true)
            {
                var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
                MainWindow = mainWindow;
                ShutdownMode = ShutdownMode.OnMainWindowClose; // Od teraz zamknięcie MainWindow zamyka aplikację
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd podczas inicjalizacji aplikacji:\n{ex.Message}", "Błąd krytyczny", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
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