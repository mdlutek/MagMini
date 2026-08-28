namespace MagMini.Application.DTOs.Customers;

public class CompanyLookupResultDto
{
    public string Name { get; set; } = string.Empty;
    public string Nip { get; set; } = string.Empty;
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? StatusVat { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public static CompanyLookupResultDto Success(string name, string nip, string? street, string? postalCode, string? city, string? statusVat) =>
        new() { IsSuccess = true, Name = name, Nip = nip, Street = street, PostalCode = postalCode, City = city, StatusVat = statusVat };

    public static CompanyLookupResultDto Failed(string error) =>
        new() { IsSuccess = false, ErrorMessage = error };
}