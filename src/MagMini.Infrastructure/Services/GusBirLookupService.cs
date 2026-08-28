using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace MagMini.Infrastructure.Services;

public class GusBirLookupService
{
    private readonly HttpClient _httpClient;
    private readonly string _serviceUrl;
    private readonly string _userKey;

    public GusBirLookupService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _serviceUrl = configuration["GusSettings:ServiceUrl"]
            ?? "https://wyszukiwarkaregontest.stat.gov.pl/wsBIR/UslugaBIRzewnPubl.svc";
        _userKey = configuration["GusSettings:UserKey"]
            ?? "abcde12345fg4567";
    }

    public async Task<(bool Success, string? Name, string? Street, string? PostalCode, string? City, string? Regon)> LookupByNipAsync(string cleanNip, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Logowanie do GUS i pobranie sesji (SID)
            var sessionId = await LoginAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(sessionId))
                return (false, null, null, null, null, null);

            // 2. Wyszukanie podmiotu po NIP
            var result = await SearchByNipAsync(sessionId, cleanNip, cancellationToken);
            return result;
        }
        catch
        {
            return (false, null, null, null, null, null);
        }
    }

    private async Task<string?> LoginAsync(CancellationToken cancellationToken)
    {
        var soapEnvelope = $@"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:ns=""http://CIS/BIR/PUBL/2014/07"">
<soap:Header xmlns:wsa=""http://www.w3.org/2005/08/addressing"">
<wsa:Action>http://CIS/BIR/PUBL/2014/07/IUslugaBIRzewnPubl/Zaloguj</wsa:Action>
<wsa:To>{_serviceUrl}</wsa:To>
</soap:Header>
<soap:Body>
<ns:Zaloguj>
<ns:pKluczUzytkownika>{_userKey}</ns:pKluczUzytkownika>
</ns:Zaloguj>
</soap:Body>
</soap:Envelope>";

        using var request = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);
        request.Content = new StringContent(soapEnvelope, Encoding.UTF8, "application/soap+xml");
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/soap+xml; charset=utf-8; action=\"http://CIS/BIR/PUBL/2014/07/IUslugaBIRzewnPubl/Zaloguj\"");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var cleanXml = ExtractSoapEnvelope(rawContent);
        if (cleanXml == null) return null;

        var doc = XDocument.Parse(cleanXml);
        return doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "ZalogujResult")?.Value;
    }

    private async Task<(bool Success, string? Name, string? Street, string? PostalCode, string? City, string? Regon)> SearchByNipAsync(string sessionId, string nip, CancellationToken cancellationToken)
    {
        var soapEnvelope = $@"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:ns=""http://CIS/BIR/PUBL/2014/07"" xmlns:dat=""http://CIS/BIR/PUBL/2014/07/DataContract"">
<soap:Header xmlns:wsa=""http://www.w3.org/2005/08/addressing"">
<wsa:Action>http://CIS/BIR/PUBL/2014/07/IUslugaBIRzewnPubl/DaneSzukajPodmioty</wsa:Action>
<wsa:To>{_serviceUrl}</wsa:To>
</soap:Header>
<soap:Body>
<ns:DaneSzukajPodmioty>
<ns:pParametryWyszukiwania>
<dat:Nip>{nip}</dat:Nip>
</ns:pParametryWyszukiwania>
</ns:DaneSzukajPodmioty>
</soap:Body>
</soap:Envelope>";

        using var request = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);
        request.Headers.Add("sid", sessionId);
        request.Content = new StringContent(soapEnvelope, Encoding.UTF8, "application/soap+xml");
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/soap+xml; charset=utf-8; action=\"http://CIS/BIR/PUBL/2014/07/IUslugaBIRzewnPubl/DaneSzukajPodmioty\"");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return (false, null, null, null, null, null);

        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var cleanXml = ExtractSoapEnvelope(rawContent);
        if (cleanXml == null) return (false, null, null, null, null, null);

        var doc = XDocument.Parse(cleanXml);
        var innerXmlResult = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "DaneSzukajPodmiotyResult")?.Value;
        if (string.IsNullOrWhiteSpace(innerXmlResult)) return (false, null, null, null, null, null);

        var innerDoc = XDocument.Parse(innerXmlResult);
        var daneElem = innerDoc.Descendants("dane").FirstOrDefault();
        if (daneElem == null || daneElem.Element("ErrorCode") != null)
            return (false, null, null, null, null, null);

        var name = daneElem.Element("Nazwa")?.Value;
        var city = daneElem.Element("Miejscowosc")?.Value;
        var postalCode = daneElem.Element("KodPocztowy")?.Value;
        var streetName = daneElem.Element("Ulica")?.Value;
        var propertyNumber = daneElem.Element("NrNieruchomosci")?.Value;
        var localNumber = daneElem.Element("NrLokalu")?.Value;
        var regon = daneElem.Element("Regon")?.Value;

        var street = streetName;
        if (!string.IsNullOrWhiteSpace(propertyNumber))
        {
            street = string.IsNullOrWhiteSpace(street) ? propertyNumber : $"{street} {propertyNumber}";
            if (!string.IsNullOrWhiteSpace(localNumber))
                street += $"/{localNumber}";
        }

        return (true, name, street, postalCode, city, regon);
    }

    /// <summary>
    /// Wyciąga czystą kopertę SOAP z wieloczęściowej odpowiedzi MTOM/MIME zwracanej przez GUS
    /// </summary>
    private static string? ExtractSoapEnvelope(string rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent)) return null;

        var startTag = "<s:Envelope";
        var endTag = "</s:Envelope>";

        var startIndex = rawContent.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
        if (startIndex == -1)
        {
            startTag = "<soap:Envelope";
            endTag = "</soap:Envelope>";
            startIndex = rawContent.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
        }

        if (startIndex == -1) return null;

        var endIndex = rawContent.IndexOf(endTag, startIndex, StringComparison.OrdinalIgnoreCase);
        if (endIndex == -1) return null;

        return rawContent.Substring(startIndex, (endIndex + endTag.Length) - startIndex);
    }
}