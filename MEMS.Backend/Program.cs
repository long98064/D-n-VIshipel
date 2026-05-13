using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MEMS.Backend.Infrastructure.Data;
using MEMS.Backend.Core.Interfaces;
using MEMS.Backend.Infrastructure.Services;
using MEMS.Backend.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Database — chỉ đăng ký 1 lần ✅ FIX: xóa bản duplicate ở dưới
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<CategorySeederService>();

// EPPlus License
OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

// 3. JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var keyString = jwtSettings["Key"];

// ✅ FIX: Ném lỗi rõ ràng nếu Key rỗng — không dùng fallback hardcode
if (string.IsNullOrWhiteSpace(keyString))
    throw new InvalidOperationException("JWT Key chưa được cấu hình. Set biến môi trường Jwt__Key.");

var key = Encoding.ASCII.GetBytes(keyString);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // ✅ True trên Production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(key),
        ValidateIssuer           = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidateAudience         = true,
        ValidAudience            = jwtSettings["Audience"],
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero
    };
});

// 4. CORS từ config ✅ FIX: không hardcode origin
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowConfiguredOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Service.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var backupPath = config["StorageSettings:BackupPath"];
    
    if (!string.IsNullOrWhiteSpace(backupPath) && !Directory.Exists(backupPath))
    {
        Directory.CreateDirectory(backupPath);
        Console.WriteLine($"[Init] Đã tự động tạo thư mục Backup: {backupPath}");
    }
}

if (app.Environment.IsDevelopment()){
    app.UseSwagger();
    ap.UseSwaggerUI();
}

// ✅ FIX: Exception Middleware — phải đứng đầu pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowConfiguredOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();