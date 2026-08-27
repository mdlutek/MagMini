using MagMini.Domain.Enums;

namespace MagMini.Application.DTOs.Articles;

public class SaveArticleDto
{
    public int Id { get; set; } // 0 dla nowego
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Ean { get; set; }
    public int CategoryId { get; set; }
    public UnitOfMeasure Unit { get; set; } = UnitOfMeasure.Pcs;
    public VatRate VatRate { get; set; } = VatRate.Vat23;
    public decimal PurchasePriceNet { get; set; }
    public decimal DefaultSalePriceNet { get; set; }
    public decimal StockQuantity { get; set; }
    public decimal MinStockQuantity { get; set; }
}