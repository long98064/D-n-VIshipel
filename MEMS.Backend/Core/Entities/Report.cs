using MEMS.Backend.Core.Entities.Base;
using MEMS.Backend.Core.Enums;

namespace MEMS.Backend.Core.Entities;

public class Report : AuditableEntity, IMustHaveBranch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal TotalAmount { get; set; } // Cộng dồn từ ReportLines
    public ReportStatus Status { get; set; } = ReportStatus.Draft;
    
    public Guid BranchId { get; set; }
    public virtual Branch Branch { get; set; } = null!;
    
    public virtual ICollection<ReportLine> Lines { get; set; } = new List<ReportLine>();
}
