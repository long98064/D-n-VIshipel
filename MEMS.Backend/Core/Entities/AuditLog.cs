using MEMS.Backend.Core.Entities.Base;

namespace MEMS.Backend.Core.Entities;

public class AuditLog : AuditableEntity, IMustHaveBranch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? Details { get; set; }
    
    public Guid BranchId { get; set; }
    public virtual Branch Branch { get; set; } = null!;
}
