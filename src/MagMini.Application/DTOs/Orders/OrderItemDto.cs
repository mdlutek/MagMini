using MagMini.Domain.Enums;

namespace MagMini.Application.DTOs.Orders;

public class OrderItemDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public string ArticleCode { get; set; } = string.Empty;
    public string ArticleName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPriceNet { get; set; }
    public VatRate VatRate { get; set; } = VatRate.Vat23;
    public decimal TotalNet => Quantity * UnitPriceNet;
    public decimal TotalGross => TotalNet * (1 + (VatRate > 0 ? (decimal)VatRate / 100m : 0m));
}