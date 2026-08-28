using MagMini.Domain.Enums;

namespace MagMini.Application.DTOs.Orders;

public class SaveOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public int CustomerId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public string? Remarks { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();

    public decimal TotalNet => Items.Sum(i => i.TotalNet);
    public decimal TotalGross => Items.Sum(i => i.TotalGross);
    public decimal TotalVat => TotalGross - TotalNet;
}