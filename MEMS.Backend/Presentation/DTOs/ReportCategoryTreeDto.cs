namespace MEMS.Backend.Presentation.DTOs;

public class ReportCategoryTreeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public int OrderIndex { get; set; }
    public bool IsLeaf { get; set; }
    
    // Trường lưu trữ tổng cộng dồn (Rollup Value)
    public decimal TotalValue { get; set; }
    
    // Chứa các nốt con (đi 1 chiều, tránh JSON Reference Loop)
    public List<ReportCategoryTreeDto> Children { get; set; } = new List<ReportCategoryTreeDto>();
}

public class SimulateRollupRequest
{
    public Dictionary<Guid, decimal> LeafValues { get; set; } = new Dictionary<Guid, decimal>();
}
