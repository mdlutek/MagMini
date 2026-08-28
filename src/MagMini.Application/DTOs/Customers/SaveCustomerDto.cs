namespace MagMini.Application.DTOs.Customers;

public class SaveCustomerDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Nip { get; set; }
    public bool IsCompany { get; set; } = true;
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}