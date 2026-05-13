using MEMS.Backend.Core.Entities.Base;

namespace MEMS.Backend.Core.Entities;

public class User : AuditableEntity, IMustHaveBranch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    // Quan hệ n-1 với Role và Branch
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    public Guid BranchId { get; set; }
    public virtual Branch Branch { get; set; } = null!;
}
