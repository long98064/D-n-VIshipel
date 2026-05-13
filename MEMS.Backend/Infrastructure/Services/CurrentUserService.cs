using System.Security.Claims;
using MEMS.Backend.Core.Interfaces;

namespace MEMS.Backend.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub") 
                             ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    // Cẩn thận tránh lỗi lúc Migration (HttpContext = null)
    public Guid? BranchId
    {
        get
        {
            var branchIdStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue("branchId");
            if (Guid.TryParse(branchIdStr, out var branchId))
            {
                return branchId;
            }
            return null; // Trả về null khi chạy migration hoặc chưa đăng nhập
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            var role = _httpContextAccessor.HttpContext?.User?.FindFirstValue("role") 
                       ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
            return role == "SuperAdmin";
        }
    }
}
