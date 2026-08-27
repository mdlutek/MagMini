namespace MagMini.Application.DTOs.Articles;

public class ArticleFilterDto
{
    public string? SearchPhrase { get; set; } // Szuka po: Symbol, Nazwa, EAN
    public int? CategoryId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 200; // Stała wielkość strony
}