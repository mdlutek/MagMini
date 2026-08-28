using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using MagMini.Application.Common.Interfaces;
using MagMini.Application.DTOs.Customers;

namespace MagMini.Infrastructure.Services;

public class HybridCompanyLookupService : ICompanyLookupService
{
    private readonly HttpClient _mfHttpClient;
    private readonly GusBirLookupService _gusService;

    public HybridCompanyLookupService(HttpClient mfHttpClient, GusBirLookupService gusService)
    {
        _mfHttpClient = mfHttpClient;
        _mfHttpClient.BaseAddress = new Uri("https://wl-api.mf.gov.pl/");
        _mfHttpClient.Timeout = TimeSpan.FromSeconds(8);
        _gusService = gusService;
    }

    public async Task<CompanyLookupResultDto> LookupByNipAsync(string nip, CancellationToken cancellationToken = default)
    {
        var cleanNip = nip.Replace("-", "").Replace(" ", "").Trim();

        // 1. POBRANIE PEŁNYCH DANYCH REJESTROWYCH Z GUS (Baza REGON)
        var gusTask = _gusService.LookupByNipAsync(cleanNip, cancellationToken);

        // 2. POBRANIE STATUSU VAT Z BIAŁEJ LISTY MF
        var mfTask = FetchFromMfAsync(cleanNip, cancellationToken);

        await Task.WhenAll(gusTask, mfTask);

        var gusResult = await gusTask;
        var mfResult = await mfTask;

        // Jeśli ani GUS, ani MF nie znalazły podmiotu
        if (!gusResult.Success && !mfResult.Success)
        {
            return CompanyLookupResultDto.Failed("Nie odnaleziono podmiotu w rejestrach państwowych (GUS / MF).");
        }

        // Fuzja danych (Preferujemy pełną nazwę i dokładny adres z GUS)
        var finalName = gusResult.Success ? gusResult.Name! : mfResult.Name!;
        var finalStreet = gusResult.Success ? gusResult.Street : mfResult.Street;
        var finalPostalCode = gusResult.Success ? gusResult.PostalCode : mfResult.PostalCode;
        var finalCity = gusResult.Success ? gusResult.City : mfResult.City;

        var finalStatusVat = mfResult.Success
            ? mfResult.StatusVat
            : "Brak w rejestrze VAT (Zwolniony / Nie-VAT)";

        return CompanyLookupResultDto.Success(
            name: finalName,
            nip: cleanNip,
            street: finalStreet,
            postalCode: finalPostalCode,
            city: finalCity,
            statusVat: finalStatusVat
        );
    }

    private async Task<(bool Success, string? Name, string? Street, string? PostalCode, string? City, string? StatusVat)> FetchFromMfAsync(string cleanNip, CancellationToken cancellationToken)
    {
        try
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var response = await _mfHttpClient.GetAsync($"api/search/nip/{cleanNip}?date={today}", cancellationToken);
            if (!response.IsSuccessStatusCode) return (false, null, null, null, null, null);

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var root = doc.RootElement;
            if (!root.TryGetProperty("result", out var resultElem) ||
                !resultElem.TryGetProperty("subject", out var subjectElem) ||
                subjectElem.ValueKind == JsonValueKind.Null)
            {
                return (false, null, null, null, null, null);
            }

            var name = subjectElem.GetProperty("name").GetString();
            var statusVat = subjectElem.TryGetProperty("statusVat", out var statusElem) ? statusElem.GetString() : null;

            string? rawAddress = null;
            if (subjectElem.TryGetProperty("workingAddress", out var workAddr) && workAddr.ValueKind == JsonValueKind.String)
                rawAddress = workAddr.GetString();
            else if (subjectElem.TryGetProperty("residenceAddress", out var resAddr) && resAddr.ValueKind == JsonValueKind.String)
                rawAddress = resAddr.GetString();

            var (street, postalCode, city) = ParseGovAddress(rawAddress);

            return (true, name, street, postalCode, city, statusVat);
        }
        catch
        {
            return (false, null, null, null, null, null);
        }
    }

    private static (string? Street, string? PostalCode, string? City) ParseGovAddress(string? rawAddress)
    {
        if (string.IsNullOrWhiteSpace(rawAddress)) return (null, null, null);

        var match = Regex.Match(rawAddress, @"(\d{2}-\d{3})\s+([^,]+)$");
        if (match.Success)
        {
            var postalCode = match.Groups[1].Value.Trim();
            var city = match.Groups[2].Value.Trim();
            var street = rawAddress.Substring(0, match.Index).Trim().TrimEnd(',');
            return (street, postalCode, city);
        }

        return (rawAddress, null, null);
    }
}