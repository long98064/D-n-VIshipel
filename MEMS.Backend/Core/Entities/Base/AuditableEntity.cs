namespace MEMS.Backend.Core.Entities.Base;

public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
}

// Core/Entities/FormSchema.cs
public class FormSchema : AuditableEntity 
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public bool IsActive { get; set; }
    // Danh sách các trường cấp 1 (ví dụ: I, II, III)
    public List<SchemaFieldDefinition> Fields { get; set; } = new();
}

public class SchemaFieldDefinition
{
    public string FieldId { get; set; } // Ví dụ: "1.1", "I"
    public string Label { get; set; }
    public string Type { get; set; } // "number", "text", "group"
    public List<SchemaFieldDefinition> Children { get; set; } = new(); // Đệ quy cho các cấp 1.1, 1.1.1
}

// Core/Entities/FormSubmission.cs
public class FormSubmission : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid SchemaId { get; set; }
    public Guid BranchId { get; set; }
    // Lưu kết quả nhập dưới dạng Dictionary: { "1.1": 500, "1.2": 300 }
    // Sử dụng Dictionary để EF Core 8 map thẳng vào JSON Column
    public Dictionary<string, object> Values { get; set; } = new();
}