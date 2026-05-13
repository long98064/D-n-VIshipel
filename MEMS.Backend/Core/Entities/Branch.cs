using MEMS.Backend.Core.Entities.Base;

namespace MEMS.Backend.Core.Entities;

public class Branch : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    
    // Navigation property
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
