using Microsoft.EntityFrameworkCore;
using MEMS.Backend.Core.Entities;
using MEMS.Backend.Core.Interfaces;
using MEMS.Backend.Infrastructure.Data;
using MEMS.Backend.Presentation.DTOs;

namespace MEMS.Backend.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _dbContext;

    public CategoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // --- THUẬT TOÁN BUILD TREE TRÊN RAM O(N) ---
    public async Task<List<ReportCategoryTreeDto>> GetCategoryTreeAsync()
    {
        // Chỉ cần 1 Query gọi xuống Database, tự động ăn Global Query Filter BranchId.
        var flatCategories = await _dbContext.ReportCategories
            .OrderBy(c => c.OrderIndex)
            .AsNoTracking()
            .ToListAsync();

        var lookup = new Dictionary<Guid, ReportCategoryTreeDto>();

        // Bước 1: Duyệt O(N) khởi tạo DTO
        foreach (var cat in flatCategories)
        {
            lookup[cat.Id] = new ReportCategoryTreeDto
            {
                Id = cat.Id,
                Code = cat.Code,
                Title = cat.Title,
                ParentId = cat.ParentId,
                OrderIndex = cat.OrderIndex,
                IsLeaf = cat.IsLeaf,
                TotalValue = 0 // Giá trị khởi tạo
            };
        }

        var rootNodes = new List<ReportCategoryTreeDto>();

        // Bước 2: Duyệt O(N) để gán con vào cha
        foreach (var cat in flatCategories)
        {
            var dto = lookup[cat.Id];

            if (cat.ParentId.HasValue && lookup.TryGetValue(cat.ParentId.Value, out var parentDto))
            {
                parentDto.Children.Add(dto);
            }
            else
            {
                // Root node (Hoặc ParentId trỏ ra ngoài bị hỏng cũng được kéo lên Root)
                rootNodes.Add(dto);
            }
        }

        return rootNodes;
    }

    // --- THUẬT TOÁN ĐỆ QUY POST-ORDER ROLLUP ---
    public void CalculateTreeRollup(List<ReportCategoryTreeDto> tree, Dictionary<Guid, decimal> leafValues)
    {
        foreach (var node in tree)
        {
            TraverseAndRollup(node, leafValues);
        }
    }

    private decimal TraverseAndRollup(ReportCategoryTreeDto node, Dictionary<Guid, decimal> leafValues)
    {
        // Tính giá trị các con trước (Từ dưới lên / Bottom-Up)
        decimal sumChildren = 0;
        foreach (var child in node.Children)
        {
            sumChildren += TraverseAndRollup(child, leafValues);
        }

        if (node.IsLeaf)
        {
            // Nếu là nốt lá, ưu tiên dữ liệu từ Dictionary (Dữ liệu đầu vào)
            node.TotalValue = leafValues.TryGetValue(node.Id, out var val) ? val : 0;
        }
        else
        {
            // Nếu là nốt cha, bằng tổng các nốt con
            node.TotalValue = sumChildren;
        }

        return node.TotalValue;
    }

    // --- THUẬT TOÁN CHỐNG VÒNG LẶP (CIRCULAR REFERENCE) ---
    public async Task ValidateCategoryHierarchyAsync(Guid currentId, Guid? newParentId)
    {
        if (!newParentId.HasValue) return;

        if (currentId == newParentId.Value)
            throw new InvalidOperationException("Bảo mật: Nốt cha không thể trỏ vào chính nó.");

        // Kéo cây gọn nhẹ nhất (Chỉ Id và ParentId) để duyệt
        var hierarchy = await _dbContext.ReportCategories
            .Select(c => new { c.Id, c.ParentId })
            .AsNoTracking()
            .ToListAsync();

        var lookup = hierarchy.ToDictionary(c => c.Id, c => c.ParentId);
        var visited = new HashSet<Guid>();
        var currentParentToCheck = newParentId;

        // Duyệt ngược từ Parent mới lên trên Root
        while (currentParentToCheck.HasValue)
        {
            // Nếu đường đi ngược lên chạm phải nốt hiện tại, tức là nốt hiện tại đang ôm Parent. Gây vòng lặp!
            if (currentParentToCheck.Value == currentId)
                throw new InvalidOperationException("Nghiệp vụ: Chuyển ParentId gây ra chu trình vòng lặp (A là cha của B, B lại là cha của A).");

            if (!visited.Add(currentParentToCheck.Value))
                throw new InvalidOperationException("Hệ thống: Phát hiện chu trình rác ngầm định trong Database.");

            if (lookup.TryGetValue(currentParentToCheck.Value, out var nextParent))
            {
                currentParentToCheck = nextParent;
            }
            else
            {
                currentParentToCheck = null;
            }
        }
    }
}
