using Microsoft.EntityFrameworkCore;
using MEMS.Backend.Infrastructure.Data;
using MEMS.Backend.Core.Interfaces;
using MEMS.Backend.Core.Entities;
using MEMS.Backend.Presentation.DTOs;

namespace MEMS.Backend.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _dbContext;

    public AdminService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // --- Branches ---
    public async Task<List<BranchDto>> GetBranchesAsync()
        => await _dbContext.Branches
            .Select(b => new BranchDto(b.Id, b.Name, b.Address))
            .ToListAsync();

    public async Task CreateBranchAsync(string name, string address)
    {
        await _dbContext.Branches.AddAsync(new Branch { Name = name, Address = address });
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteBranchAsync(Guid id)
    {
        var branch = await _dbContext.Branches.FindAsync(id);
        if (branch != null)
        {
            _dbContext.Branches.Remove(branch);
            await _dbContext.SaveChangesAsync();
        }
    }

    // --- Users ---
    public async Task<List<UserDto>> GetUsersAsync()
        => await _dbContext.Set<User>()
            .Select(u => new UserDto(u.Id, u.Username, u.FullName, u.Role.Name, u.RoleId, u.BranchId))
            .ToListAsync();

    public async Task<List<RoleDto>> GetRolesAsync()
        => await _dbContext.Set<Role>()
            .Select(r => new RoleDto(r.Id, r.Name))
            .ToListAsync();

    public async Task CreateUserAsync(string username, string password, Guid roleId, Guid branchId)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        await _dbContext.Set<User>().AddAsync(new User
        {
            Username     = username,
            PasswordHash = hashedPassword, 
            FullName     = username,
            RoleId       = roleId,
            BranchId     = branchId
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _dbContext.Set<User>().FindAsync(id);
        if (user != null)
        {
            _dbContext.Set<User>().Remove(user);
            await _dbContext.SaveChangesAsync();
        }
    }

    // --- Categories ---
    public async Task CreateCategoryAsync(string title, string code, Guid? parentId, bool isLeaf)
    {
        await _dbContext.ReportCategories.AddAsync(new ReportCategory
        {
            Title    = title,
            Code     = code,
            ParentId = parentId,
            IsLeaf   = isLeaf
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var cat = await _dbContext.ReportCategories.FindAsync(id);
        if (cat != null)
        {
            _dbContext.ReportCategories.Remove(cat);
            await _dbContext.SaveChangesAsync();
        }
    }

    // --- Audit Logs & Backup ---
    public async Task<PagedResult<AuditLog>> GetAuditLogsAsync(int pageIndex, int pageSize, string? actionFilter)
    {
        var query = _dbContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrEmpty(actionFilter))
            query = query.Where(x => x.Action.Contains(actionFilter));

        int totalRecords = await query.CountAsync();
        int totalPages   = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var logs = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Data          = logs,
            TotalRecords  = totalRecords,
            TotalPages    = totalPages,
            CurrentPage   = pageIndex
        };
    }

   public async Task BackupDatabaseAsync(string backupPath)
        {
            var dbName = _context.Database.GetDbConnection().Database;
            var fileName = $"{dbName}_Backup_{DateTime.Now:yyyyMMddHHmmss}.bak";
            var fullBackupPath = Path.Combine(backupPath, fileName);
            var pathParam = new SqlParameter("@backupPath", fullBackupPath);

            var sqlCommand = $"BACKUP DATABASE [{dbName}] TO DISK = @backupPath";

            await _context.Database.ExecuteSqlRawAsync(sqlCommand, pathParam);
        }
}