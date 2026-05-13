using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MEMS.Backend.Core.Interfaces;
using MEMS.Backend.Presentation.DTOs;

namespace MEMS.Backend.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // ✅ Bắt buộc authentication cho tất cả endpoints
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Lấy danh sách reports theo năm/tháng
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetReports(
        [FromQuery] int year,
        [FromQuery] int? month = null)
    {
        try
        {
            var reports = await _reportService.GetReportsByYearMonthAsync(year, month);
            return Ok(reports);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy chi tiết 1 report
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReportById(Guid id)
    {
        try
        {
            var report = await _reportService.GetReportByIdAsync(id);
            if (report == null)
                return NotFound(new { error = "Báo cáo không tồn tại." });

            return Ok(report);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Upload báo cáo từ Excel
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] string title,
        [FromForm] int month,
        [FromForm] int year,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File không hợp lệ.");
        
        try
        {
            // ✅ FIX: Trả về UploadReportResponse (DTO) thay vì Guid
            var result = await _reportService.UploadReportAsync(title, month, year, file);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            // Lỗi có kiểm soát (VD: không tìm thấy data, file lỗi)
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            // Lỗi hệ thống (DB, nền tảng)
            return StatusCode(500, new { error = "Lỗi hệ thống: " + ex.Message });
        }
    }

    /// <summary>
    /// Approve (chuyển trạng thái) báo cáo
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "SuperAdmin,Manager")] // ✅ Chỉ SuperAdmin/Manager mới approve được
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            // ✅ FIX: Trả về ApproveReportResponse (DTO) thay vì string message
            var result = await _reportService.ApproveReportAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Lỗi hệ thống: " + ex.Message });
        }
    }
}