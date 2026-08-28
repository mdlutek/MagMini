using System.Net.Http.Headers;

namespace MagMini.Mobile.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    public AuthHeaderHandler()
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Automatyczne pobranie tokenu z telefonu
        var token = await SecureStorage.Default.GetAsync("jwt_token");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}