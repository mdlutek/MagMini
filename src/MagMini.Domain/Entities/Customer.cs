using MagMini.Domain.Common;

namespace MagMini.Domain.Entities;

public class Customer : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty; // Symbol kontrahenta
    public string Name { get; set; } = string.Empty;
    public string? Nip { get; set; }
    public bool IsCompany { get; set; }

    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}