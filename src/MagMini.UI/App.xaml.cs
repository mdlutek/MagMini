using System.Windows;
using MagMini.Application.Common.Interfaces;
using MagMini.Infrastructure;
using MagMini.Infrastructure.Persistence;
using MagMini.UI.Services;
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

                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var splash = new SplashScreenView();
        splash.Show();

        try
        {
            // Cała ciężka praca leci w osobnym wątku (Task.Run), wątek UI pozostaje w 100% płynny
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
                await Task.Delay(400); // Krótki bufor wizualny
            });

            var mainWindow = AppHost!.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            splash.Close();
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