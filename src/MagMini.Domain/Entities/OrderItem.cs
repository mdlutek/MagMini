using MagMini.Domain.Common;
using MagMini.Domain.Enums;

namespace MagMini.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public decimal Quantity { get; set; }
    public decimal UnitPriceNet { get; set; }
    public VatRate VatRate { get; set; }

    // Wartości wyliczane
    public decimal TotalNet => Quantity * UnitPriceNet;
    public decimal TotalGross => TotalNet * (1 + (VatRate > 0 ? (decimal)VatRate / 100m : 0m));
}