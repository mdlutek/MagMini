using MagMini.Domain.Enums;

namespace MagMini.Application.DTOs.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string? CustomerNip { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusDescription => Status switch
    {
        OrderStatus.Draft => "Robocze",
        OrderStatus.Confirmed => "Zatwierdzone",
        OrderStatus.Completed => "Zrealizowane",
        OrderStatus.Cancelled => "Anulowane",
        _ => "Nieznany"
    };

    public decimal TotalNet { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalVat => TotalGross - TotalNet;
    public int ItemsCount { get; set; }
    public string? CreatedBy { get; set; }
}