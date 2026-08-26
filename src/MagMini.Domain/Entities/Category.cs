using MagMini.Domain.Common;

namespace MagMini.Domain.Entities;

public class Category : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Article> Articles { get; set; } = new List<Article>();
}