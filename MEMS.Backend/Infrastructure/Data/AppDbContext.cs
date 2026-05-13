using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MEMS.Backend.Core.Entities;
using MEMS.Backend.Core.Entities.Base;
using MEMS.Backend.Core.Interfaces;

namespace MEMS.Backend.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) 
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ReportCategory> ReportCategories { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<ReportLine> ReportLines { get; set; }
    public DbSet<YearlyTarget> YearlyTargets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Lấy tất cả Entity trong ModelBuilder
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Select(t => t.ClrType)
            .ToList();

        foreach (var clrType in entityTypes)
        {
            // 1. Áp dụng Reflection để apply Global Query Filter tự động cho mọi Entity
            // Điều kiện: Implement cả IMustHaveBranch và AuditableEntity
            if (typeof(IMustHaveBranch).IsAssignableFrom(clrType) && 
                typeof(AuditableEntity).IsAssignableFrom(clrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ApplyGlobalQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.MakeGenericMethod(clrType);

                method?.Invoke(this, new object[] { modelBuilder });
            }
            // Nếu Entity chỉ kế thừa AuditableEntity mà không có IMustHaveBranch (như Role, Branch)
            else if (typeof(AuditableEntity).IsAssignableFrom(clrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.MakeGenericMethod(clrType);

                method?.Invoke(this, new object[] { modelBuilder });
            }

            // 2. Tự động đánh Index bằng Fluent API
            if (typeof(IMustHaveBranch).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).HasIndex(nameof(IMustHaveBranch.BranchId));
            }
            if (typeof(AuditableEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).HasIndex(nameof(AuditableEntity.IsDeleted));
            }
        }
        // =========================================================
        // DATA SEEDING: Tự động seed dữ liệu mầu khi Migration
        // Mật khẩu được hash bằng BCrypt cùng thuật toán vs AuthController
        // =========================================================
        var adminRoleId = new Guid("11111111-1111-1111-1111-111111111111");
        var branchId    = new Guid("22222222-2222-2222-2222-222222222222");
        var adminUserId = new Guid("33333333-3333-3333-3333-333333333333");
        var now         = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Role>().HasData(new Role
        {
            Id = adminRoleId,
            Name = "Admin",
            CreatedAt = now,
            CreatedBy = "System",
            IsDeleted = false
        });

        modelBuilder.Entity<Branch>().HasData(new Branch
        {
            Id = branchId,
            Name = "Trụ sở chính",
            Address = "Nguyễn Thượng Hiền, Hải Phòng",
            CreatedAt = now,
            CreatedBy = "System",
            IsDeleted = false
        });

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = adminUserId,
            Username = "admin",
            FullName = "Quản trị viên",
            // BCrypt.HashPassword với WorkFactor mặc định (11)
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            RoleId = adminRoleId,
            BranchId = branchId,
            CreatedAt = now,
            CreatedBy = "System",
            IsDeleted = false
        });
    }

    // Helper method apply filter kết hợp Soft Delete và Branch Isolation
    private void ApplyGlobalQueryFilter<T>(ModelBuilder modelBuilder) where T : AuditableEntity, IMustHaveBranch
    {
        // Global Query Filter được evaluate mỗi khi query. 
        // EF Core sẽ tự động inject _currentUserService vào cây biểu thức.
        modelBuilder.Entity<T>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_currentUserService.IsSuperAdmin || e.BranchId == _currentUserService.BranchId));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
    base.OnModelCreating(builder);

    modelBuilder.Entity<FormSchema>(b =>
    {
        b.ToTable("Form_Schemas");
        // Ánh xạ JSON cho danh sách Fields động
        b.OwnsMany(x => x.Fields, fieldsBuilder =>
        {
            fieldsBuilder.ToJson(); 
        });
    });

    modelBuilder.Entity<FormSubmission>(b =>
    {
        b.ToTable("Form_Submissions");
        // Ánh xạ JSON cho Payload Data (Key-Value)
        b.OwnsOne(x => x.PayloadData, payloadBuilder =>
        {
            payloadBuilder.ToJson();
        });
    });
    }

    // Helper method apply filter Soft Delete cơ bản cho entity không chia nhánh
    private void ApplySoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : AuditableEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Xử lý null an toàn (ví dụ: background worker, migration hoặc thao tác ban đầu chưa có user)
        var userId = _currentUserService.UserId ?? "System";
        var branchId = _currentUserService.BranchId;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = userId;
                    break;

                case EntityState.Deleted:
                    // Cơ chế Soft Delete: Chặn xóa cứng, chuyển thành Modify và set IsDeleted
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }

        // Tự động gán BranchId từ Token cho các record mới, giúp đảm bảo Zero-Trust Input
        // Tuyệt đối không cho phép DTO từ client truyền lên BranchId
        foreach (var entry in ChangeTracker.Entries<IMustHaveBranch>())
        {
            if (entry.State == EntityState.Added)
            {
                // Chỉ gán nếu current token thuộc một nhánh cụ thể (không phải migration)
                if (branchId.HasValue && !entry.Entity.BranchId.Equals(Guid.Empty))
                {
                    // Ưu tiên branchId từ claims. Nếu chưa có thì overwrite để đảm bảo cách ly dữ liệu.
                    entry.Entity.BranchId = branchId.Value;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
