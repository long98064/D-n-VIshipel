namespace MEMS.Backend.Core.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    Guid? BranchId { get; }
    bool IsSuperAdmin { get; }
}
