namespace MagMini.Application.DTOs.Customers;

public class CustomerDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty; // Symbol kontrahenta
    public string Name { get; set; } = string.Empty;
    public string? Nip { get; set; }
    public bool IsCompany { get; set; }
    public string TypeDescription => IsCompany ? "Firma" : "Osoba prywatna";

    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Street { get; set; }
    public string FullAddress => $"{PostalCode} {City}, {Street}".Trim();

    public string? Phone { get; set; }
    public string? Email { get; set; }
}