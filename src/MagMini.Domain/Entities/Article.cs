using MagMini.Domain.Common;
using MagMini.Domain.Enums;

namespace MagMini.Domain.Entities;

public class Article : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty; // SKU / Symbol w WAPRO
    public string Name { get; set; } = string.Empty;
    public string? Ean { get; set; }
    public UnitOfMeasure Unit { get; set; } = UnitOfMeasure.Pcs;
    public VatRate VatRate { get; set; } = VatRate.Vat23;

    public decimal PurchasePriceNet { get; set; }
    public decimal DefaultSalePriceNet { get; set; }
    public decimal DefaultSalePriceGross => DefaultSalePriceNet * (1 + (VatRate > 0 ? (decimal)VatRate / 100m : 0m));

    public decimal StockQuantity { get; set; }
    public decimal MinStockQuantity { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}