using Microsoft.AspNetCore.Http;
using MEMS.Backend.Presentation.DTOs;

namespace MEMS.Backend.Core.Interfaces;

public interface IReportService
{
    /// <summary>
    /// Upload báo cáo từ file Excel
    /// </summary>
    Task<UploadReportResponse> UploadReportAsync(string title, int month, int year, IFormFile file);

    /// <summary>
    /// Approve (chuyển trạng thái) báo cáo
    /// </summary>
    Task<ApproveReportResponse> ApproveReportAsync(Guid reportId);

    /// <summary>
    /// Lấy chi tiết báo cáo với tất cả dòng
    /// </summary>
    Task<ReportDto?> GetReportByIdAsync(Guid reportId);

    /// <summary>
    /// Liệt kê báo cáo theo năm/tháng
    /// </summary>
    Task<List<ReportDto>> GetReportsByYearMonthAsync(int year, int? month);
}