namespace MagMini.Application.DTOs.Customers;

public class CustomerFilterDto
{
    public string? SearchPhrase { get; set; } // Szuka po: Symbol, Nazwa, NIP, Miasto
    public bool? IsCompanyOnly { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 200;
}