using MagMini.Mobile.Services;
using MagMini.Mobile.ViewModels;
using MagMini.Mobile.Views;

namespace MagMini.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. Rejestracja interceptora
        builder.Services.AddTransient<AuthHeaderHandler>();

        // 2. Klient autoryzacji (do logowania)
        builder.Services.AddHttpClient<AuthService>(client =>
        {
            client.BaseAddress = new Uri(ApiConfig.BaseUrl);
        }).ConfigurePrimaryHttpMessageHandler(() => DevHttpsHandler.GetPlatformMessageHandler());

        // 3. Klient towarów (z automatycznym doklejaniem JWT i obsługą deweloperską HTTPS)
        builder.Services.AddHttpClient<ArticleMobileService>(client =>
        {
            client.BaseAddress = new Uri(ApiConfig.BaseUrl);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>()
        .ConfigurePrimaryHttpMessageHandler(() => DevHttpsHandler.GetPlatformMessageHandler());

        // Rejestracja klienta zamówień
        builder.Services.AddHttpClient<OrderMobileService>(client =>
        {
            client.BaseAddress = new Uri(ApiConfig.BaseUrl);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>()
        .ConfigurePrimaryHttpMessageHandler(() => DevHttpsHandler.GetPlatformMessageHandler());

        // 4. ViewModele i Strony
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();

        builder.Services.AddTransient<ArticlesViewModel>();
        builder.Services.AddTransient<ArticlesPage>();

        builder.Services.AddTransient<DashboardMobileViewModel>();
        builder.Services.AddTransient<DashboardPage>();

        builder.Services.AddTransient<OrdersViewModel>();
        builder.Services.AddTransient<OrdersPage>();
        builder.Services.AddTransient<OrderDetailViewModel>();
        builder.Services.AddTransient<OrderDetailPage>();

        builder.Services.AddTransient<ScannerViewModel>();
        builder.Services.AddTransient<ScannerPage>();

        return builder.Build();
    }
}