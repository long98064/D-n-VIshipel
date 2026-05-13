using MEMS.Backend.Core.Entities.Base;

namespace MEMS.Backend.Core.Entities;

public class ReportLine : AuditableEntity, IMustHaveBranch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReportId { get; set; }
    public virtual Report Report { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public virtual ReportCategory Category { get; set; } = null!;

    public decimal Amount { get; set; } // VND
    public string? Note { get; set; }   // Text: Ví dụ "Tỷ giá: 25000"

    public Guid BranchId { get; set; }
}
