using MEMS.Backend.Core.Entities.Base;

namespace MEMS.Backend.Core.Entities;

public class YearlyTarget : AuditableEntity, IMustHaveBranch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Year { get; set; }
    public Guid CategoryId { get; set; }
    public virtual ReportCategory Category { get; set; } = null!;
    public decimal TargetAmount { get; set; }
    
    public Guid BranchId { get; set; }
}
