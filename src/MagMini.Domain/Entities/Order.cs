using MagMini.Domain.Common;
using MagMini.Domain.Enums;

namespace MagMini.Domain.Entities;

public class Order : BaseAuditableEntity
{
    public string OrderNumber { get; set; } = string.Empty; // np. ZK/2026/03/001
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public string? Remarks { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public decimal TotalNet => Items.Sum(i => i.TotalNet);
    public decimal TotalGross => Items.Sum(i => i.TotalGross);
}