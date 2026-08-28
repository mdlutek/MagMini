namespace MagMini.Mobile.Services;

public static class ApiConfig
{
    // Wpisz port HTTPS swojego API (np. 7156 - sprawdź port w Swaggerze!)
    public const int ApiPort = 7156;

    public static string BaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? $"https://10.0.2.2:{ApiPort}/"
            : $"https://localhost:{ApiPort}/";
}