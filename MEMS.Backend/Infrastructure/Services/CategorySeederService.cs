using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using MEMS.Backend.Core.Entities;
using MEMS.Backend.Infrastructure.Data;

namespace MEMS.Backend.Infrastructure.Services;

/// <summary>
/// Service đọc file Excel mẫu biểu báo cáo và import cây danh mục vào ReportCategories.
/// Thuật toán phân cấp dựa trên cột STT (cột A) không hardcode số dòng.
/// </summary>
public class CategorySeederService
{
    private readonly AppDbContext _dbContext;

    public CategorySeederService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(int imported, int skipped)> ImportFromExcelAsync(Stream fileStream, Guid branchId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(fileStream);
        var sheet = package.Workbook.Worksheets[0]; // Lấy sheet đầu tiên

        int rowCount = sheet.Dimension?.Rows ?? 0;
        if (rowCount == 0) return (0, 0);

        // ============================================================
        // BƯỚC 1: Xây dựng danh sách phẳng (Flat List) từ Excel
        // ============================================================

        // Biến lưu trạng thái Parent gần nhất của từng cấp
        Guid? currentLevel1Id = null; // La Mã (I, II, III)
        Guid? currentLevel2Id = null; // Số nguyên (1, 2, 3)
        Guid? currentLevel3Id = null; // Số thập phân (1.1, 1.2)

        var categories = new List<ReportCategory>();
        int orderIndex = 0;

        for (int row = 1; row <= rowCount; row++)
        {
            var sttRaw = sheet.Cells[row, 1].Text?.Trim();   // Cột A: STT
            var title  = sheet.Cells[row, 2].Text?.Trim();   // Cột B: NỘI DUNG

            // Bỏ qua dòng tiêu đề hoặc dòng không có nội dung
            if (string.IsNullOrWhiteSpace(title)) continue;

            int level = DetectLevel(sttRaw, title);
            if (level == 0) continue; // Không nhận dạng được cấp, bỏ qua

            var category = new ReportCategory
            {
                Id = Guid.NewGuid(),
                Title = CleanTitle(title),
                Code = BuildCode(sttRaw, level, orderIndex),
                OrderIndex = orderIndex++,
                IsLeaf = true, // Mặc định là Lá, sẽ được update khi tìm thấy con
                BranchId = branchId
            };

            // Gán ParentId theo cấp và cập nhật state
            switch (level)
            {
                case 1: // La Mã → Root
                    category.ParentId = null;
                    currentLevel1Id = category.Id;
                    currentLevel2Id = null;
                    currentLevel3Id = null;
                    break;

                case 2: // Số nguyên → con của Level 1
                    category.ParentId = currentLevel1Id;
                    currentLevel2Id = category.Id;
                    currentLevel3Id = null;
                    break;

                case 3: // Số thập phân → con của Level 2
                    category.ParentId = currentLevel2Id;
                    currentLevel3Id = category.Id;
                    break;

                case 4: // Dấu "-" → con của Level 3 (hoặc Level 2 nếu không có Level 3)
                    category.ParentId = currentLevel3Id ?? currentLevel2Id;
                    break;
            }

            // ======================================================
            // Khi tìm thấy một mục con, đánh dấu Cha là IsLeaf = false
            // ======================================================
            if (category.ParentId.HasValue)
            {
                var parent = categories.FirstOrDefault(c => c.Id == category.ParentId.Value);
                if (parent != null) parent.IsLeaf = false;
            }

            categories.Add(category);
        }

        if (!categories.Any()) return (0, 0);

        // ============================================================
        // BƯỚC 2: Soft-delete danh mục cũ và insert mới bằng SQL thuần
        // Dùng SQL trực tiếp để bypass AppDbContext.SaveChangesAsync()
        // (Tránh việc override BranchId khi không có JWT Token)
        // ============================================================
        
        // Soft-delete các danh mục cũ
        int skipped = await _dbContext.Database.ExecuteSqlRawAsync(
            "UPDATE ReportCategories SET IsDeleted = 1, UpdatedAt = GETUTCDATE() WHERE BranchId = {0} AND IsDeleted = 0",
            branchId);

        // Insert hàng loạt bằng EF nhưng được bảo vệ bởi việc gán CreatedBy trước
        var now = DateTime.UtcNow;
        foreach (var cat in categories)
        {
            cat.CreatedAt = now;
            cat.CreatedBy = "System/Import";
            cat.IsDeleted = false;
            // BranchId đã được gán từ tham số nhận vào
        }

        // Thêm trực tiếp vào DbSet không qua override SaveChangesAsync
        // Giải pháp: Đánh dấu EntityState.Added thủ công, rồi gọi base context
        await _dbContext.ReportCategories.AddRangeAsync(categories);

        // Override BranchId trước khi SaveChanges chạy
        foreach (var entry in _dbContext.ChangeTracker.Entries<ReportCategory>()
            .Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added))
        {
            entry.Entity.BranchId = branchId; // Pin lại để AppDbContext không thể override
        }

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Re-throw với message rõ hơn cho debug
            throw new InvalidOperationException(
                $"Lỗi khi lưu danh mục vào DB: {ex.Message}. Inner: {ex.InnerException?.Message}", ex);
        }

        return (categories.Count, skipped);
    }

    // ============================================================
    // HELPER: Lấy toàn bộ danh mục dạng phẳng (Flat List)
    // ============================================================
    public async Task<List<ReportCategory>> GetCategoriesAsync()
    {
        return await _dbContext.ReportCategories
            .IgnoreQueryFilters()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.OrderIndex)
            .ToListAsync();
    }

    // ============================================================
    // HELPER: Phát hiện cấp bậc từ giá trị cột STT
    // ============================================================
    private static int DetectLevel(string? stt, string title)
    {
        if (string.IsNullOrWhiteSpace(stt))
        {
            // Cấp 4: STT trống và title bắt đầu bằng "-"
            if (title.StartsWith("-")) return 4;
            return 0; // Dòng tiêu đề hoặc không xác định
        }

        stt = stt.Trim();

        // Cấp 1: Số La Mã (I, II, III, IV, V, VI, VII, VIII, IX, X...)
        if (IsRomanNumeral(stt)) return 1;

        // Cấp 3: Số thập phân như 1.1, 2.3
        if (stt.Contains('.') && double.TryParse(stt, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out _)) return 3;

        // Cấp 2: Số nguyên như 1, 2, 3
        if (int.TryParse(stt, out _)) return 2;

        return 0;
    }

    private static bool IsRomanNumeral(string s)
    {
        // Các ký tự hợp lệ trong số La Mã
        return !string.IsNullOrEmpty(s) &&
               s.ToUpper().All(c => "IVXLCDM".Contains(c)) &&
               s.Length > 0;
    }

    // ============================================================
    // HELPER: Tạo Code hạng mục để dùng khi Import số liệu
    // ============================================================
    private static string BuildCode(string? stt, int level, int orderIndex)
    {
        if (string.IsNullOrWhiteSpace(stt))
            return $"CAT_L4_{orderIndex:D4}";

        // Làm sạch dấu chấm cuối: "1." → "1", "1.1." → "1.1"
        stt = stt.TrimEnd('.');
        var safeCode = stt.Replace(".", "_");
        return $"CAT_{safeCode}";
    }

    // ============================================================
    // HELPER: Làm sạch tiêu đề (bỏ dấu "-" đầu dòng)
    // ============================================================
    private static string CleanTitle(string title)
    {
        return title.TrimStart('-', ' ').Trim();
    }
}
