using MEMS.Backend.Presentation.DTOs;

namespace MEMS.Backend.Core.Interfaces;

public interface IAdminService
{
    // Branches
    Task<List<BranchDto>> GetBranchesAsync();
    Task CreateBranchAsync(string name, string address);
    Task DeleteBranchAsync(Guid id);

    // Users
    Task<List<UserDto>> GetUsersAsync();
    Task<List<RoleDto>> GetRolesAsync();
    Task CreateUserAsync(string username, string password, Guid roleId, Guid branchId);
    Task DeleteUserAsync(Guid id);

    // Categories
    Task CreateCategoryAsync(string title, string code, Guid? parentId, bool isLeaf);
    Task DeleteCategoryAsync(Guid id);

    // Audit Logs & Backup
    Task<PagedResult<Core.Entities.AuditLog>> GetAuditLogsAsync(int pageIndex, int pageSize, string? actionFilter);
    Task<string> CreateDatabaseBackupAsync();

    //Backup
    Task BackupDatabaseAsync(string backupPath);
}