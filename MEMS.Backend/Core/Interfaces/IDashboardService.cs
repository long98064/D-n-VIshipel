using System.Threading.Tasks;
using MEMS.Backend.Presentation.DTOs; // Giả định Namespace chứa DTOs

namespace MEMS.Backend.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<UrgentTasksDto> GetUrgentTasksAsync(int branchId);
        Task<KpiSummaryDto> GetKpiSummaryAsync(int branchId, int year);
        Task<ChartDataResponseDto> GetChartDataAsync(int branchId, int year);
        Task<InsightsDto> GetInsightsAsync(int branchId, int year);
        Task<PagedResult<ReportHistoryDto>> GetReportHistoryAsync(int branchId, int page, int limit);
    }
}