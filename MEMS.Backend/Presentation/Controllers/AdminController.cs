using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MEMS.Backend.Core.Interfaces;

namespace MEMS.Backend.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readdonly IConfiguration _configuration;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
        _configuration = configuration;
    }

    // --- Branches ---
    [HttpGet("branches")]
    public async Task<IActionResult> GetBranches() => Ok(await _adminService.GetBranchesAsync());

    [HttpPost("branches")]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchReq req)
    {
        await _adminService.CreateBranchAsync(req.Name, req.Address);
        return Ok();
    }

    [HttpDelete("branches/{id}")]
    public async Task<IActionResult> DeleteBranch(Guid id)
    {
        await _adminService.DeleteBranchAsync(id);
        return Ok();
    }

    // --- Users ---
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers() => Ok(await _adminService.GetUsersAsync());

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles() => Ok(await _adminService.GetRolesAsync());

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserReq req)
    {
        await _adminService.CreateUserAsync(req.Username, req.Password, req.RoleId, req.BranchId);
        return Ok();
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _adminService.DeleteUserAsync(id);
        return Ok();
    }

    // --- Categories ---
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryReq req)
    {
        await _adminService.CreateCategoryAsync(req.Title, req.Code, req.ParentId, req.IsLeaf);
        return Ok();
    }

    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        await _adminService.DeleteCategoryAsync(id);
        return Ok();
    }

    // --- Audit Logs & Backup ---
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? actionFilter = null)
    {
        var result = await _adminService.GetAuditLogsAsync(pageIndex, pageSize, actionFilter);
        return Ok(result);
    }

  [HttpPost("backup")]
        public async Task<IActionResult> BackupDatabase()
        {
            var backupPath = _configuration["StorageSettings:BackupPath"];
            
            if (string.IsNullOrWhiteSpace(backupPath))
            {
                return StatusCode(500, new { message = "Lỗi cấu hình: Chưa thiết lập đường dẫn lưu bản sao lưu." });
            }

            await _adminService.BackupDatabaseAsync(backupPath);
            return Ok(new { message = "Sao lưu dữ liệu thành công!" });
        }
}

// Request Models — sẽ chuyển vào DTOs ở Đợt 2
public record CreateBranchReq(string Name, string Address);
public record CreateUserReq(string Username, string Password, Guid RoleId, Guid BranchId);
public record CreateCategoryReq(string Title, string Code, Guid? ParentId, bool IsLeaf);