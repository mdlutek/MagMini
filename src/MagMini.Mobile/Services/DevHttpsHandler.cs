namespace MagMini.Mobile.Services;

public static class DevHttpsHandler
{
    public static HttpMessageHandler GetPlatformMessageHandler()
    {
        var handler = new HttpClientHandler();

        // Pomijanie weryfikacji lokalnego certyfikatu HTTPS w trybie deweloperskim
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

        return handler;
    }
}