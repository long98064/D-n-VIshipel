using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MEMS.Backend.Core.Interfaces;
using MEMS.Backend.Infrastructure.Data;
using MEMS.Backend.Presentation.DTOs;
using MEMS.Backend.Core.Enums;
using System.Collections.Generic;

namespace MEMS.Backend.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UrgentTasksDto> GetUrgentTasksAsync(int branchId)
        {
            // Lấy các báo cáo bị từ chối hoặc cần xử lý gấp
            var urgentReports = await _context.Reports
                .Where(x => x.BranchId == branchId && x.Status == ReportStatus.Rejected)
                .Select(x => new UrgentTaskItem
                {
                    ReportId = x.Id,
                    Message = $"Báo cáo tháng {x.Month}/{x.Year} đã bị từ chối. Vui lòng cập nhật lại!",
                    Type = "error"
                })
                .ToListAsync();

            return new UrgentTasksDto { Tasks = urgentReports };
        }

        public async Task<KpiSummaryDto> GetKpiSummaryAsync(int branchId, int year)
        {
            // Tối ưu Performance: Sử dụng TotalRevenue từ Entity Report, không JOIN ReportLine
            var totalRevenue = await _context.Reports
                .Where(x => x.BranchId == branchId && x.Year == year && x.Status == ReportStatus.Approved)
                .SumAsync(x => x.TotalRevenue);

            var yearlyTarget = await _context.YearlyTargets
                .Where(x => x.BranchId == branchId && x.Year == year)
                .Select(x => x.TargetAmount)
                .FirstOrDefaultAsync();

            var completionRate = yearlyTarget > 0 ? (totalRevenue / yearlyTarget) * 100 : 0;

            return new KpiSummaryDto
            {
                TotalRevenue = totalRevenue,
                TargetAmount = yearlyTarget,
                CompletionRate = Math.Round(completionRate, 2),
                IsGrowth = true // Logic YoY có thể mở rộng thêm bằng cách so sánh với year - 1
            };
        }

        public async Task<ChartDataResponseDto> GetChartDataAsync(int branchId, int year)
        {
            var reports = await _context.Reports
                .Where(x => x.BranchId == branchId && x.Year == year)
                .Select(x => new { x.Month, x.TotalRevenue })
                .ToListAsync();

            var yearlyTarget = await _context.YearlyTargets
                .FirstOrDefaultAsync(x => x.BranchId == branchId && x.Year == year);

            var monthlyTarget = yearlyTarget != null ? yearlyTarget.TargetAmount / 12 : 0;

            var chartData = Enumerable.Range(1, 12).Select(month => new ChartDataPoint
            {
                Month = $"T{month}",
                ActualAmount = reports.FirstOrDefault(r => r.Month == month)?.TotalRevenue ?? 0,
                TargetAmount = monthlyTarget
            }).ToList();

            return new ChartDataResponseDto { Data = chartData };
        }

        public async Task<InsightsDto> GetInsightsAsync(int branchId, int year)
        {
            // Dummy logic tạo rule AI/Insights (Có thể mở rộng thêm)
            var insights = new List<InsightItem>
            {
                new InsightItem { Message = "Doanh thu tháng này vượt mức kỳ vọng 15%.", Color = "success" },
                new InsightItem { Message = "Chi phí vận hành đang có dấu hiệu tăng nhanh.", Color = "error" }
            };

            return new InsightsDto { Insights = insights };
        }

        public async Task<PagedResult<ReportHistoryDto>> GetReportHistoryAsync(int branchId, int page, int limit)
        {
            var query = _context.Reports
                .Where(x => x.BranchId == branchId)
                .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * limit).Take(limit)
                .Select(x => new ReportHistoryDto
                {
                    Id = x.Id,
                    Period = $"{x.Month}/{x.Year}",
                    TotalRevenue = x.TotalRevenue,
                    SubmittedBy = x.CreatedBy,
                    Status = x.Status.ToString()
                })
                .ToListAsync();

            return new PagedResult<ReportHistoryDto>(items, totalCount, page, limit);
        }
    }
}