using MEMS.Backend.Core.Entities.Base;

namespace MEMS.Backend.Core.Entities;

public class ReportCategory : AuditableEntity, IMustHaveBranch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    
    // Khóa ngoại trỏ đến Nốt cha. Null nghĩa là Nốt gốc (Root).
    public Guid? ParentId { get; set; }
    public int OrderIndex { get; set; }
    public bool IsLeaf { get; set; }
    
    public Guid BranchId { get; set; }
}
