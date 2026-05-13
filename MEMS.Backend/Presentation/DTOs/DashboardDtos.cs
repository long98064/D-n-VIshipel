namespace MEMS.Backend.Presentation.DTOs;

/// <summary>
/// DTO cho 1 dòng tóm tắt danh mục (Category Summary)
/// </summary>
public record CategorySummaryDto(
    string CategoryName,
    decimal TargetAmount,
    decimal AchievedAmount
)
{
    /// <summary>
    /// Tỷ lệ hoàn thành (%)
    /// </summary>
    public decimal CompletionPercentage => TargetAmount > 0 
        ? (AchievedAmount / TargetAmount) * 100 
        : 0;
}

/// <summary>
/// DTO cho Dashboard Summary (Tóm tắt tổng quát)
/// </summary>
public class DashboardSummaryDto
{
    public decimal TotalTarget { get; set; }
    public decimal TotalAchieved { get; set; }
    
    /// <summary>
    /// Danh sách chi tiết từng danh mục
    /// </summary>
    public List<CategorySummaryDto> Details { get; set; } = new();

    /// <summary>
    /// Tỷ lệ hoàn thành chung (%)
    /// </summary>
    public decimal OverallCompletionPercentage => TotalTarget > 0 
        ? (TotalAchieved / TotalTarget) * 100 
        : 0;
}