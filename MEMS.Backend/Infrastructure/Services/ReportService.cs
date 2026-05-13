using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using MEMS.Backend.Core.Entities;
using MEMS.Backend.Core.Enums;
using MEMS.Backend.Core.Interfaces;
using MEMS.Backend.Infrastructure.Data;
using MEMS.Backend.Presentation.DTOs;

namespace MEMS.Backend.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _dbContext;

    public ReportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UploadReportResponse> UploadReportAsync(string title, int month, int year, IFormFile file)
    {
        // 1. Lấy danh sách Category Code -> CategoryId trên RAM (Chống N+1 Query)
        var categoryMap = await _dbContext.ReportCategories
            .IgnoreQueryFilters() // Bypass BranchId filter để lấy được danh mục khi gọi không có Token
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .ToDictionaryAsync(c => c.Code, c => c.Id);

        if (categoryMap.Count == 0)
            throw new InvalidOperationException(
                "Chưa có danh mục nào trong hệ thống. Vui lòng import danh mục trước qua API POST /api/categories/import-from-excel.");

        var report = new Report
        {
            Title = title,
            Month = month,
            Year = year,
            Status = ReportStatus.Draft,
            TotalAmount = 0
        };

        var reportLines = new List<ReportLine>();

        // 2. Chống tràn RAM: Dùng Streaming với EPPlus
        using (var stream = file.OpenReadStream())
        using (var package = new ExcelPackage(stream))
        {
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null) throw new InvalidOperationException("File Excel không có Worksheet nào.");

            int rowCount = worksheet.Dimension?.Rows ?? 0;

            // 3. Đọc dữ liệu linh hoạt: Không phụ thuộc index dòng
            // Bỏ qua dòng Header (Giả sử Header ở dòng 1)
            for (int row = 2; row <= rowCount; row++)
            {
                var codeCell = worksheet.Cells[row, 1].Text?.Trim(); // Cột A: Code
                
                // Tránh lỗi khi đọc dòng trống cuối file
                if (string.IsNullOrEmpty(codeCell)) continue;

                // Đối chiếu chuỗi Code tự động
                if (categoryMap.TryGetValue(codeCell, out var categoryId))
                {
                    var amountText = worksheet.Cells[row, 2].Text?.Trim(); // Cột B: Amount
                    var note = worksheet.Cells[row, 3].Text?.Trim();       // Cột C: Note

                    if (decimal.TryParse(amountText, out decimal amount))
                    {
                        reportLines.Add(new ReportLine
                        {
                            ReportId = report.Id,
                            CategoryId = categoryId,
                            Amount = amount,
                            Note = note
                        });
                        
                        report.TotalAmount += amount; // Rollup cơ bản cho Report
                    }
                }
            }
        }

        if (!reportLines.Any()) throw new InvalidOperationException("Không tìm thấy dữ liệu hợp lệ trong file Excel.");

        // 4. Xử lý Transaction Locking (Đảm bảo an toàn DML)
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            await _dbContext.Reports.AddAsync(report);
            await _dbContext.ReportLines.AddRangeAsync(reportLines);
            
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // ✅ FIX: Trả v�� DTO thay vì Guid
            return new UploadReportResponse(
                report.Id,
                report.Title,
                report.Month,
                report.Year,
                report.TotalAmount
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ApproveReportResponse> ApproveReportAsync(Guid reportId)
    {
        var report = await _dbContext.Reports.FirstOrDefaultAsync(r => r.Id == reportId);
        if (report == null) throw new KeyNotFoundException("Không tìm thấy báo cáo.");

        var oldStatus = report.Status;
        
        // Cập nhật trạng thái
        if (report.Status == ReportStatus.Draft)
            report.Status = ReportStatus.Pending;
        else if (report.Status == ReportStatus.Pending)
            report.Status = ReportStatus.Approved;

        // Lưu Audit Log thay đổi trạng thái
        var auditLog = new AuditLog
        {
            Action = "ApproveReport",
            EntityName = nameof(Report),
            Details = $"Chuyển trạng thái từ {oldStatus} sang {report.Status}"
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            _dbContext.Reports.Update(report);
            await _dbContext.AuditLogs.AddAsync(auditLog);
            
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // ✅ FIX: Trả về DTO thay vì void
            return new ApproveReportResponse(
                report.Id,
                report.Status.ToString(),
                DateTime.UtcNow
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ReportDto?> GetReportByIdAsync(Guid reportId)
    {
        var report = await _dbContext.Reports
            .Include(r => r.Lines)
            .ThenInclude(l => l.Category)
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null) return null;

        // ✅ FIX: Map sang DTO
        return new ReportDto(
            report.Id,
            report.Title,
            report.Month,
            report.Year,
            report.Status.ToString(),
            report.TotalAmount,
            report.Lines.Select(l => new ReportLineDto(
                l.Id,
                l.CategoryId,
                l.Category.Code,
                l.Category.Title,
                l.Amount,
                l.Note
            )).ToList()
        );
    }

    public async Task<List<ReportDto>> GetReportsByYearMonthAsync(int year, int? month)
    {
        var query = _dbContext.Reports
            .Include(r => r.Lines)
            .ThenInclude(l => l.Category)
            .Where(r => r.Year == year);

        if (month.HasValue)
            query = query.Where(r => r.Month == month.Value);

        var reports = await query.ToListAsync();

        // ✅ FIX: Map sang List<ReportDto>
        return reports.Select(r => new ReportDto(
            r.Id,
            r.Title,
            r.Month,
            r.Year,
            r.Status.ToString(),
            r.TotalAmount,
            r.Lines.Select(l => new ReportLineDto(
                l.Id,
                l.CategoryId,
                l.Category.Code,
                l.Category.Title,
                l.Amount,
                l.Note
            )).ToList()
        )).ToList();
    }
}