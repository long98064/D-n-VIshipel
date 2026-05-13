namespace MEMS.Backend.Presentation.DTOs;

public class CategorySummaryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal AchievedAmount { get; set; }
    public decimal CompletionRate => TargetAmount > 0 ? (AchievedAmount / TargetAmount) * 100 : 0;
}

public class DashboardSummaryDto
{
    public decimal TotalTarget { get; set; }
    public decimal TotalAchieved { get; set; }
    public decimal OverallCompletionRate => TotalTarget > 0 ? (TotalAchieved / TotalTarget) * 100 : 0;
    
    public List<CategorySummaryDto> Details { get; set; } = new List<CategorySummaryDto>();
}
