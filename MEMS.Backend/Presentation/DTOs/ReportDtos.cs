namespace MEMS.Backend.Presentation.DTOs;

/// <summary>
/// Response khi upload báo cáo từ Excel
/// </summary>
public record UploadReportResponse(Guid ReportId, string Title, int Month, int Year, decimal TotalAmount);

/// <summary>
/// DTO cho Report Line (Dòng chi tiết báo cáo)
/// </summary>
public record ReportLineDto(
    Guid Id,
    Guid CategoryId,
    string CategoryCode,
    string CategoryTitle,
    decimal Amount,
    string? Note
);

/// <summary>
/// DTO cho Report (Báo cáo với danh sách dòng)
/// </summary>
public record ReportDto(
    Guid Id,
    string Title,
    int Month,
    int Year,
    string Status,
    decimal TotalAmount,
    List<ReportLineDto> Lines
);

/// <summary>
/// Response khi approve báo cáo
/// </summary>
public record ApproveReportResponse(Guid ReportId, string NewStatus, DateTime ApprovedAt);