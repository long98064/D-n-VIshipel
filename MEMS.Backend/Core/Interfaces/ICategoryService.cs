using MEMS.Backend.Presentation.DTOs;

namespace MEMS.Backend.Core.Interfaces;

public interface ICategoryService
{
    Task<List<ReportCategoryTreeDto>> GetCategoryTreeAsync();
    void CalculateTreeRollup(List<ReportCategoryTreeDto> tree, Dictionary<Guid, decimal> leafValues);
    Task ValidateCategoryHierarchyAsync(Guid currentId, Guid? newParentId);
}
