using MagMini.Domain.Enums;

namespace MagMini.Application.DTOs.Orders;

public class OrderFilterDto
{
    public string? SearchPhrase { get; set; } // Numer dokumentu, kontrahent, NIP
    public OrderStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 200;
}