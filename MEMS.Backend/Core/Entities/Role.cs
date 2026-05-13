using MEMS.Backend.Core.Entities.Base;

namespace MEMS.Backend.Core.Entities;

public class Role : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty; // Ví dụ: "SuperAdmin", "Manager", "User"
    
    // Navigation property
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
