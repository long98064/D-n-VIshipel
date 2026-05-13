using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MEMS.Backend.Core.Interfaces;

namespace MEMS.Backend.Presentation.Controllers
{
    [ApiController]
    [Route("api/v2/[controller]")]
    [Authorize] // Bật lại bảo mật
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentUserService _currentUserService;

        public DashboardController(IDashboardService dashboardService, ICurrentUserService currentUserService)
        {
            _dashboardService = dashboardService;
            _currentUserService = currentUserService;
        }

        [HttpGet("urgent-tasks")]
        public async Task<IActionResult> GetUrgentTasks()
        {
            var data = await _dashboardService.GetUrgentTasksAsync(_currentUserService.BranchId);
            return Ok(data);
        }

        [HttpGet("kpi-summary")]
        public async Task<IActionResult> GetKpiSummary([FromQuery] int year)
        {
            if (year <= 0) year = System.DateTime.UtcNow.Year; // Fallback
            var data = await _dashboardService.GetKpiSummaryAsync(_currentUserService.BranchId, year);
            return Ok(data);
        }

        [HttpGet("chart-data")]
        public async Task<IActionResult> GetChartData([FromQuery] int year)
        {
            if (year <= 0) year = System.DateTime.UtcNow.Year;
            var data = await _dashboardService.GetChartDataAsync(_currentUserService.BranchId, year);
            return Ok(data);
        }

        [HttpGet("insights")]
        public async Task<IActionResult> GetInsights([FromQuery] int year)
        {
            if (year <= 0) year = System.DateTime.UtcNow.Year;
            var data = await _dashboardService.GetInsightsAsync(_currentUserService.BranchId, year);
            return Ok(data);
        }

        // Endpoint này gộp vào DashboardController để tiện phục vụ nguyên cụm UI Dashboard như Spec yêu cầu
        [HttpGet("history")]
        public async Task<IActionResult> GetReportHistory([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var data = await _dashboardService.GetReportHistoryAsync(_currentUserService.BranchId, page, limit);
            return Ok(data);
        }
    }
}