using Microsoft.AspNetCore.Mvc;
using MEMS.Backend.Infrastructure.Services;
using MEMS.Backend.Core.Interfaces;

namespace MEMS.Backend.Presentation.Controllers;

[ApiController]
[Route("api/v2/[Controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly CategorySeederService _seederService;
    private readonly ICurrentUserService _currentUserService;

    public CategoriesController(CategorySeederService seederService, ICurrentUserService currentUserService)
    {
        _seederService = seederService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {       
            // Lấy ID người dùng từ JWT Token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                // Bắt lỗi 401 ngay lập tức nếu không trích xuất được ID
                return Unauthorized(new { message = "Truy cập bị từ chối: Không xác định được danh tính người dùng." });
            }

            // Tiếp tục truyền userId hợp lệ xuống Service
            var result = await _categoryService.CreateCategoryAsync(dto, userId);
            return Ok(result);
        }
}
