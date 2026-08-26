namespace MagMini.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity, IAuditableEntity
{
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}