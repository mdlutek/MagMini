using MagMini.Domain.Enums;

namespace MagMini.Application.DTOs.Articles;

public class ArticleDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Ean { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public UnitOfMeasure Unit { get; set; }
    public VatRate VatRate { get; set; }
    public decimal PurchasePriceNet { get; set; }
    public decimal DefaultSalePriceNet { get; set; }
    public decimal DefaultSalePriceGross { get; set; }
    public decimal StockQuantity { get; set; }
    public decimal MinStockQuantity { get; set; }
}