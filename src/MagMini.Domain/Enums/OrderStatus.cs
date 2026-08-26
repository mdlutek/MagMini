namespace MagMini.Domain.Enums;

public enum OrderStatus
{
    Draft = 0,       // Wersja robocza / Nowe
    Confirmed = 1,   // Zatwierdzone (rezerwacja towaru)
    Completed = 2,   // Zrealizowane / Wydane
    Cancelled = 3    // Anulowane
}