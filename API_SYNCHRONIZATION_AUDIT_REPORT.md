# 📋 BÁO CÁO KIỂM TOÁN & ĐỒNG BỘ HÓA API - MEMS V2.0
**Ngày Báo Cáo:** 2026-05-13  
**Trạng Thái:** ⚠️ PHÁT HIỆN NHIỀU LỖI NGHIÊM TRỌNG  
**Độ Ưu Tiên:** 🔴 URGENT - Cần sửa ngay trước khi Deploy

---

## 📑 MỤC LỤC
1. [Bản Đồ API (API Mapping Discovery)](#1-bản-đồ-api--api-mapping--discovery)
2. [Bất Đồng Bộ Cấu Trúc Dữ Liệu](#2-bất-đồng-bộ-cấu-trúc-dữ-liệu--dto--payload-mismatches)
3. [Kiểm Tra HTTP Methods & Status Codes](#3-kiểm-tra-http-methods--status-codes)
4. [Vấn Đề Base URL & CORS](#4-vấn-đề-base-url--cors)
5. [Danh Sách Lỗi Nghiêm Trọng & Checklist Sửa Lỗi](#5-danh-sách-lỗi-nghiêm-trọng--checklist-sửa-lỗi)
6. [Code Sửa Lỗi Chi Tiết](#6-code-sửa-lỗi-chi-tiết)

---

## 1. Bản Đồ API (API Mapping & Discovery)

### 📡 Backend Endpoints (Từ Controllers)

| Controller | Route | HTTP Method | Path Đầy Đủ | Ghi Chú |
|-----------|-------|-------------|-------------|---------|
| **Auth** | `/api/[controller]` | POST | `/api/auth/login` | ✅ ĐÚNG |
| **Admin** | `/api/[controller]` | GET | `/api/admin/branches` | ✅ ĐÚNG |
| **Admin** | `/api/[controller]` | POST | `/api/admin/branches` | ✅ ĐÚNG |
| **Admin** | `/api/[controller]` | DELETE | `/api/admin/branches/{id}` | ✅ ĐÚNG |
| **Admin** | `/api/[controller]` | GET | `/api/admin/users` | ✅ ĐÚNG |
| **Admin** | `/api/[controller]` | POST | `/api/admin/users` | ✅ ĐÚNG |
| **Admin** | `/api/[controller]` | DELETE | `/api/admin/users/{id}` | ✅ ĐÚNG |
| **Admin** | `/api/[controller]` | GET | `/api/admin/roles` | ✅ ĐÚNG |
| **Admin** | `/api/[controller]` | POST | `/api/admin/categories` | ✅ ĐÚNG |
| **Admin** | `/api/[controller]` | DELETE | `/api/admin/categories/{id}` | ✅ ĐÚNG |
| **Admin** | `/api/[controller]` | GET | `/api/admin/audit-logs` | ✅ ĐÚNG |
| **Admin** | `/api/[controller]` | POST | `/api/admin/backup` | ✅ ĐÚNG (LƯU Ý: Frontend gọi sai) |
| **Categories** | `/api/v2/[Controller]` | POST | `/api/v2/categories` | ⚠️ ROUTE CONFLICT! Chữ "Controller" viết Hoa |
| **Reports** | `/api/[controller]` | GET | `/api/reports` | ✅ ĐÚNG |
| **Reports** | `/api/[controller]` | GET | `/api/reports/{id}` | ✅ ĐÚNG |
| **Reports** | `/api/[controller]` | POST | `/api/reports/upload` | ✅ ĐÚNG |
| **Reports** | `/api/[controller]` | POST | `/api/reports/{id}/approve` | ✅ ĐÚNG |
| **Dashboard** | `/api/v2/[controller]` | GET | `/api/v2/dashboard/urgent-tasks` | ✅ ĐÚNG |
| **Dashboard** | `/api/v2/[controller]` | GET | `/api/v2/dashboard/kpi-summary` | ✅ ĐÚNG |
| **Dashboard** | `/api/v2/[controller]` | GET | `/api/v2/dashboard/chart-data` | ✅ ĐÚNG |
| **Dashboard** | `/api/v2/[controller]` | GET | `/api/v2/dashboard/insights` | ✅ ĐÚNG |
| **Dashboard** | `/api/v2/[controller]` | GET | `/api/v2/dashboard/history` | ✅ ĐÚNG |

### 📱 Frontend API Calls (Từ Components & Stores)

| Component/Store | Endpoint Gọi | HTTP Method | Trạng Thái |
|-----------------|-------------|------------|----------|
| LoginPage.jsx | `/auth/login` | POST | ✅ ĐÚNG |
| BranchManager.jsx | `/admin/branches` | GET | ✅ ĐÚNG |
| BranchManager.jsx | `/admin/branches` | POST | ✅ ĐÚNG |
| BranchManager.jsx | `/admin/branches/{id}` | DELETE | ✅ ĐÚNG |
| UserManager.jsx | `/admin/users` | GET | ✅ ĐÚNG |
| UserManager.jsx | `/admin/users` | POST | ✅ ĐÚNG |
| UserManager.jsx | `/admin/users/{id}` | DELETE | ✅ ĐÚNG |
| UserManager.jsx | `/admin/roles` | GET | ✅ ĐÚNG |
| UserManager.jsx | `/admin/branches` | GET | ✅ ĐÚNG |
| AdminPage.jsx | `/admin/backup/trigger` | POST | ❌ **SAI** - Backend không có endpoint này! |
| Dashboard.jsx | `/api/v2/dashboard/urgent-tasks` | GET | ❌ **SAI** - Đường dẫn thừa `/api` |
| Dashboard.jsx | `/api/v2/dashboard/kpi-summary` | GET | ❌ **SAI** - Đường dẫn thừa `/api` |
| Dashboard.jsx | `/api/v2/dashboard/chart-data` | GET | ❌ **SAI** - Đường dẫn thừa `/api` |
| Dashboard.jsx | `/api/v2/dashboard/insights` | GET | ❌ **SAI** - Đường dẫn thừa `/api` |
| Dashboard.jsx | `/api/v2/dashboard/history` | GET | ❌ **SAI** - Đường dẫn thừa `/api` |

### 🔴 **LỖI NGHIÊM TRỌNG #1: API ROUTING CHAOS**

**Vấn Đề:**
- ❌ **Dashboard.jsx** gọi `/api/v2/dashboard/...` nhưng `axiosClient.baseURL` đã là `http://localhost:5000/api`
- Kết quả thực tế: Gọi đến `http://localhost:5000/api/api/v2/dashboard/...` → **404 Not Found**
- ❌ **AdminPage.jsx** gọi `/admin/backup/trigger` nhưng Backend định nghĩa là `/admin/backup`

**Thiệt Hại:**
- 💥 Dashboard hoàn toàn không hoạt động
- 💥 Backup tính năng không chạy

---

## 2. Bất Đồng Bộ Cấu Trúc Dữ Liệu (DTO & Payload Mismatches)

### 🔴 **LỖI NGHIÊM TRỌNG #2: NAMING CONVENTION CHAOS - Snake Case vs PascalCase**

#### A. Login Endpoint Mismatches

**Backend DTOs (LoginRequest.cs):**
```csharp
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}
```

**Frontend Gửi (LoginPage.jsx):**
```javascript
const response = await axiosClient.post('/auth/login', {
    username: values.username,  // ✅ lowercase - ĐÚNG
    password: values.password   // ✅ lowercase - ĐÚNG
});
```

**Kết Luận:** ✅ **OK** - Axios auto-chuyển đổi, nhưng KHÔNG nên dựa vào điều này

---

#### B. Branch Management Mismatches

**Backend DTOs (AdminDtos.cs):**
```csharp
public record BranchDto(Guid Id, string Name, string Address);
```

**Backend Endpoint (AdminController.cs, Line 26):**
```csharp
[HttpPost("branches")]
public async Task<IActionResult> CreateBranch([FromBody] CreateBranchReq req)
{
    await _adminService.CreateBranchAsync(req.Name, req.Address);
    return Ok();  // ❌ KHÔNG TRẢ VỀ DTO! Trả Ok() trống
}
```

**Frontend Gửi (BranchManager.jsx, Line 30):**
```javascript
await axiosClient.post('/admin/branches', values);
// values = { name: "...", address: "..." }  ✅ lowercase - ĐÚng
```

**Frontend Mong Đợi (BranchManager.jsx, Line 14):**
```javascript
const res = await axiosClient.get('/admin/branches');
setBranches(res);  // Expect: [ { id, name, address }, ... ]
```

**Kết Luận:** 
- ⚠️ **RISKY** - CreateBranch chỉ trả `Ok()` mà không trả DTO
- ✅ GetBranches trả về OK (hiểu rằng trả BranchDto[])

---

#### C. User Management Mismatches

**Backend DTOs (AdminDtos.cs):**
```csharp
public record UserDto(Guid Id, string Username, string FullName, string RoleName, Guid RoleId, Guid BranchId);
```

**Backend Response (AdminController.cs, Line 41):**
```csharp
[HttpGet("users")]
public async Task<IActionResult> GetUsers() => Ok(await _adminService.GetUsersAsync());
// Trả: UserDto[]
```

**Frontend Mong Đợi (UserManager.jsx, Line 16, 60):**
```javascript
const res = await axiosClient.get('/admin/users');
setUsers(res);
// Render: dataIndex: 'username', 'roleName', 'branchId'
```

**Kết Luận:** ✅ **OK** - Nhưng phải xác nhận rõ UserDto trả ra có các field này

---

#### D. 🔴 **LỖI NGHIÊM TRỌNG #3: Dashboard Payload Type Mismatch**

**Backend Trả Về (Tinh Định):**
```javascript
// /api/v2/dashboard/urgent-tasks
{
  tasks: [ { reportId, message, type } ]
}

// /api/v2/dashboard/kpi-summary
{
  totalRevenue: number,
  targetAmount: number,
  completionRate: number,
  isGrowth: boolean
}
```

**Frontend Mong Đợi (Dashboard.jsx, Lines 52-67):**
```javascript
const [urgentRes, kpiRes, chartRes, insightsRes, historyRes] = 
  await Promise.all([
    axiosClient.get('/api/v2/dashboard/urgent-tasks'),
    axiosClient.get(`/api/v2/dashboard/kpi-summary?year=${year}`),
    // ...
  ]);

setUrgentTasks(urgentRes.data.tasks || []);  // ⚠️ urgentRes.data.tasks - LẤY PROPERTY "data"
setKpiSummary(kpiRes.data || {});           // ⚠️ kpiRes.data - LẤY PROPERTY "data"
```

**Kết Luận:**
- ⚠️ **CRITICAL BUG** - Axios interceptor trả `response.data` (Line 28 axiosClient.js)
- Frontend lại `.data` thêm lần nữa → `res.data.data.tasks`
- Backend trả `{ tasks: [...] }` nhưng Frontend code chờ `response.data.tasks`
- **Hậu quả:** Dashboard data toàn `undefined`

**Đây là BUG CHAIN:**
1. axiosClient interceptor: `return response.data` → stripped response wrapper
2. Frontend expect: `urgentRes.data` → nhưng `urgentRes` đã là unwrapped data
3. Result: `urgentRes` = `{ tasks: [...] }`, `urgentRes.data` = `undefined`

---

### 🔴 **LỖI NGHIÊM TRỌNG #4: JWT Claims Parsing Mismatch**

**Backend Tạo Claims (AuthController.cs, Lines 48-55):**
```csharp
var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    new Claim("role", user.Role?.Name ?? "User"),
    new Claim("branchId", user.BranchId.ToString()),
    new Claim("fullName", user.FullName),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
};
```

**Frontend Parse Claims (useAuthStore.js, Lines 18-20):**
```javascript
user: {
    userId: decoded.sub || decoded.nameid,
    role: decoded.role || decoded.Role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
    branchId: decoded.branchId || decoded.BranchId || decoded.branchid
},
```

**Kết Luận:**
- ✅ OK - Frontend defensive coding với fallback
- Nhưng `decoded.fullName` không được lưu → Dashboard không hiểu `user.fullName`
- **Line 118 Dashboard.jsx:** `user?.fullName` = `undefined`

---

## 3. Kiểm Tra HTTP Methods & Status Codes

### ✅ HTTP Methods - Tổng quan OK

| Endpoint | Backend | Frontend | Status |
|----------|---------|----------|--------|
| GET /branches | ✅ | ✅ | MATCH |
| POST /branches | ✅ | ✅ | MATCH |
| DELETE /branches/{id} | ✅ | ✅ | MATCH |
| GET /users | ✅ | ✅ | MATCH |
| POST /users | ✅ | ✅ | MATCH |
| DELETE /users/{id} | ✅ | ✅ | MATCH |

### 🔴 **Status Code Handling Issues**

**Backend (Program.cs & Controllers):**
```csharp
// Status 200 OK
return Ok(data);

// Status 201 Created (KHÔNG DÙNG - nên dùng cho POST tạo resource)
// ❌ Thiếu

// Status 400 Bad Request
return BadRequest(new { error = ex.Message });

// Status 401 Unauthorized
return Unauthorized("Tài khoản hoặc mật khẩu không đúng.");

// Status 403 Forbidden
return StatusCode(403);  // ❌ Không dùng, nên dùng Forbid()

// Status 404 Not Found
return NotFound(new { error = "Báo cáo không tồn tại." });

// Status 500 Internal Server Error
return StatusCode(500, new { error = "Lỗi hệ thống: " + ex.Message });
```

**Frontend (axiosClient.js - Response Interceptor):**
```javascript
axiosClient.interceptors.response.use(
    (response) => {
        return response.data;  // ✅ Unwrap response
    },
    (error) => {
        if (error.response) {
            const status = error.response.status;
            // Đá ra Login nếu Token hết hạn hoặc không có quyền
            if (status === 401 || status === 403) {
                useAuthStore.getState().logout();
                window.location.href = '/login';
            }
        }
        return Promise.reject(error);
    }
);
```

**Kết Luận:**
- ✅ 401/403 handling OK
- ⚠️ Không handle 201 Created
- ⚠️ Không xử lý Response wrapper format chuẩn (API nên trả `{ success, data, message }`)

---

## 4. Vấn Đề Base URL & CORS

### Backend Configuration (Program.cs, Lines 61-74)

```csharp
// ✅ CORS từ config - GOOD
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
              .AllowCredentials();  // ✅ Cho phép gửi Cookies/JWT
    });
});
```

### Backend Settings (appsettings.json, Line 18)

```json
"AllowedOrigins": [
    "http://localhost:5173"
]
```

### Frontend Configuration (axiosClient.js, Line 5)

```javascript
const axiosClient = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL,
    headers: {
        'Content-Type': 'application/json'
    }
});
```

### 🔴 **LỖI NGHIÊM TRỌNG #5: MISSING .env Configuration**

**Vấn Đề:**
- `import.meta.env.VITE_API_BASE_URL` không định nghĩa ở đâu
- Nếu không có `.env` file hoặc biến này, baseURL = `undefined`
- Axios sẽ gọi relative URLs → **Gọi đến http://localhost:5173/api/...** (Frontend port!)

**Không có .env.example hoặc hướng dẫn setup**

---

### 🔴 **LỖI NGHIÊM TRỌNG #6: AdminController Constructor Bug**

**AdminController.cs, Line 13:**
```csharp
private readdonly IConfiguration _configuration;  // ❌ Typo: "readdonly" thay vì "readonly"
```

**Line 15-19:**
```csharp
public AdminController(IAdminService adminService)
{
    _adminService = adminService;
    _configuration = configuration;  // ❌ Parameter "configuration" không tồn tại!
}
```

**Kết Luận:** 
- 💥 **COMPILE ERROR** - Đây là lỗi syntax sẽ ngăn project build
- Backup endpoint (Line 89) cần `_configuration` nên sẽ null reference exception

---

### 🔴 **LỖI NGHIÊM TRỌNG #7: Program.cs Syntax Errors**

**Program.cs, Line 82:**
```csharp
using (var scope = app.Service.CreateScope())
                            // ❌ "Service" - TYPO! Phải là "Services"
```

**Program.cs, Lines 94-96:**
```csharp
if (app.Environment.IsDevelopment()){
    app.UseSwagger();
    ap.UseSwaggerUI();  // ❌ "ap" - TYPO! Phải là "app"
}
```

**Program.cs, Lines 104-106 - DUPLICATE:**
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// ⚠️ Swagger middleware gọi 2 lần - lãng phí resource
```

**Kết Luận:** 
- 💥 **COMPILE ERROR** - Project sẽ không build được!

---

### 🔴 **LỖI NGHIÊM TRỌNG #8: CategoriesController Bugs**

**CategoriesController.cs, Line 8:**
```csharp
[Route("api/v2/[Controller]")]  // ❌ "[Controller]" viết Hoa
                                // Sẽ thành "/api/v2/Categories" (Controllers suffix bị loại)
```

**Line 9:**
```csharp
[Authorize]  // ❌ Missing: using Microsoft.AspNetCore.Authorization;
```

**Line 25:**
```csharp
var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
// ❌ Missing: using System.Security.Claims;
```

**Line 34:**
```csharp
var result = await _categoryService.CreateCategoryAsync(dto, userId);
// ❌ "_categoryService" không được inject! Constructor chỉ có:
// - CategorySeederService
// - ICurrentUserService
```

**Kết Luận:**
- 💥 **MULTIPLE COMPILE ERRORS**
- 🔴 CategorySeederService != ICategoryService
- Routes sẽ thành `/api/v2/Categories` thay vì `/api/v2/categories`

---

## 5. Danh Sách Lỗi Nghiêm Trọng & Checklist Sửa Lỗi

### 🔴 PRIORITY CRITICAL (CẦN SỬA NGAY - DEPLOY BLOCKED)

| # | Lỗi | File | Dòng | Ưu Tiên | Ước Lượng |
|---|-----|------|------|---------|----------|
| 1 | Program.cs: `app.Service` → `app.Services` TYPO | Program.cs | 82 | 🔴 CRITICAL | 1 phút |
| 2 | Program.cs: `ap.UseSwaggerUI()` → `app.UseSwaggerUI()` TYPO | Program.cs | 96 | 🔴 CRITICAL | 1 phút |
| 3 | AdminController: `readdonly` → `readonly` TYPO | AdminController.cs | 13 | 🔴 CRITICAL | 1 phút |
| 4 | AdminController: Constructor parameter `configuration` không tồn tại | AdminController.cs | 18 | 🔴 CRITICAL | 2 phút |
| 5 | CategoriesController: Missing using directives & DI injection lỗi | CategoriesController.cs | Multi | 🔴 CRITICAL | 5 phút |
| 6 | Dashboard.jsx: Endpoint paths thừa `/api` prefix | Dashboard.jsx | 53-57 | 🔴 CRITICAL | 5 phút |
| 7 | AdminPage.jsx: Backup endpoint sai (`/admin/backup/trigger` vs `/admin/backup`) | AdminPage.jsx | 21 | 🔴 CRITICAL | 1 phút |
| 8 | Dashboard.jsx: Axios response unwrapping double `.data` bug | Dashboard.jsx | 60-67 | 🔴 CRITICAL | 10 phút |

### 🟠 PRIORITY HIGH (CẦN SỬA TRƯỚC ALPHA RELEASE)

| # | Lỗi | File | Dòng | Ưu Tiên | Ước Lượng |
|---|-----|------|------|---------|----------|
| 9 | Missing `.env` file & VITE_API_BASE_URL configuration | .env (missing) | - | 🟠 HIGH | 5 phút |
| 10 | useAuthStore: `fullName` không được lưu từ JWT | useAuthStore.js | 17-22 | 🟠 HIGH | 2 phút |
| 11 | CategoriesController: Route `[Controller]` viết Hoa | CategoriesController.cs | 8 | 🟠 HIGH | 1 phút |
| 12 | AdminController.CreateBranch: Chỉ trả `Ok()` không trả DTO | AdminController.cs | 29 | 🟠 HIGH | 2 phút |
| 13 | ReportsController: Response format không consistent | ReportsController.cs | Multi | 🟠 HIGH | 10 phút |
| 14 | Response Interceptor: Không xử lý error response body | axiosClient.js | 30-42 | 🟠 HIGH | 5 phút |

### 🟡 PRIORITY MEDIUM (NÊN SỬA TRƯỚC PRODUCTION)

| # | Lỗi | File | Dòng | Ưu Tiên | Ước Lượng |
|---|-----|------|------|---------|----------|
| 15 | Swagger middleware gọi 2 lần (Lines 94-96 & 104-106) | Program.cs | 94-106 | 🟡 MEDIUM | 1 phút |
| 16 | Không trả 201 Created cho POST endpoints | Controllers | Multi | 🟡 MEDIUM | 5 phút |
| 17 | JWT token expiration handling không strict | useAuthStore.js | 40-46 | 🟡 MEDIUM | 3 phút |
| 18 | API response format không standardized (status, message, data) | Controllers | Multi | 🟡 MEDIUM | 20 phút |

---

## 6. Code Sửa Lỗi Chi Tiết

### ✅ FIX #1: Program.cs - Sửa Typos & Duplicate Swagger

**File:** `MEMS.Backend/Program.cs`

```csharp
// ❌ TRƯỚC (Lines 82, 96, 94-106)
using (var scope = app.Service.CreateScope())  // ❌ TYPO
{
    // ...
}

if (app.Environment.IsDevelopment()){
    app.UseSwagger();
    ap.UseSwaggerUI();  // ❌ TYPO
}

// ... dòng 104-106
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ SAU
using (var scope = app.Services.CreateScope())  // ✅ FIX: Services
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var backupPath = config["StorageSettings:BackupPath"];
    
    if (!string.IsNullOrWhiteSpace(backupPath) && !Directory.Exists(backupPath))
    {
        Directory.CreateDirectory(backupPath);
        Console.WriteLine($"[Init] Đã tự động tạo thư mục Backup: {backupPath}");
    }
}

// ✅ FIX: Chỉ gọi Swagger 1 lần, xóa duplicate
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();  // ✅ FIX: app (không phải ap)
}
```

**Commit Message:**
```
Fix: Program.cs - Sửa typos app.Service → app.Services, ap → app, xóa duplicate Swagger middleware
```

---

### ✅ FIX #2: AdminController.cs - Sửa Syntax Errors & DI

**File:** `MEMS.Backend/Presentation/Controllers/AdminController.cs`

```csharp
// ❌ TRƯỚC
private readdonly IConfiguration _configuration;  // ❌ Typo + lỗi DI

public AdminController(IAdminService adminService)
{
    _adminService = adminService;
    _configuration = configuration;  // ❌ Parameter không tồn tại
}

// ✅ SAU
private readonly IConfiguration _configuration;  // ✅ FIX: readonly

public AdminController(IAdminService adminService, IConfiguration configuration)  // ✅ Add parameter
{
    _adminService = adminService;
    _configuration = configuration;  // ✅ Assign correctly
}

// ✅ FIX #2b: CreateBranch - Trả về DTO thay vì Ok()
[HttpPost("branches")]
public async Task<IActionResult> CreateBranch([FromBody] CreateBranchReq req)
{
    var result = await _adminService.CreateBranchAsync(req.Name, req.Address);
    return CreatedAtAction(nameof(GetBranches), new { }, result);  // ✅ Trả 201 + DTO
}

// ✅ FIX #2c: CreateUser - Trả DTO
[HttpPost("users")]
public async Task<IActionResult> CreateUser([FromBody] CreateUserReq req)
{
    var result = await _adminService.CreateUserAsync(req.Username, req.Password, req.RoleId, req.BranchId);
    return CreatedAtAction(nameof(GetUsers), new { }, result);  // ✅ Trả 201 + DTO
}

// ✅ FIX #2d: CreateCategory - Trả DTO
[HttpPost("categories")]
public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryReq req)
{
    var result = await _adminService.CreateCategoryAsync(req.Title, req.Code, req.ParentId, req.IsLeaf);
    return CreatedAtAction("GetCategories", new { }, result);  // ✅ Trả 201 + DTO
}
```

**Commit Message:**
```
Fix: AdminController.cs - Sửa typo readonly, thêm DI cho IConfiguration, trả về 201 Created + DTO
```

---

### ✅ FIX #3: CategoriesController.cs - Sửa Route & Missing Using

**File:** `MEMS.Backend/Presentation/Controllers/CategoriesController.cs`

```csharp
// ❌ TRƯỚC
using Microsoft.AspNetCore.Mvc;
using MEMS.Backend.Infrastructure.Services;
using MEMS.Backend.Core.Interfaces;

namespace MEMS.Backend.Presentation.Controllers;

[ApiController]
[Route("api/v2/[Controller]")]  // ❌ [Controller] viết Hoa
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly CategorySeederService _seederService;
    private readonly ICurrentUserService _currentUserService;

    public CategoriesController(CategorySeederService seederService, ICurrentUserService currentUserService)
    {
        _seederService = seederService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {       
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;  // ❌ ClaimTypes không import
        
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new { message = "Truy cập bị từ chối: Không xác định được danh tính người dùng." });
        }

        var result = await _categoryService.CreateCategoryAsync(dto, userId);  // ❌ _categoryService không tồn tại!
        return Ok(result);
    }
}

// ✅ SAU
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;  // ✅ Add
using System.Security.Claims;  // ✅ Add
using MEMS.Backend.Infrastructure.Services;
using MEMS.Backend.Core.Interfaces;
using MEMS.Backend.Presentation.DTOs;  // ✅ Add

namespace MEMS.Backend.Presentation.Controllers;

[ApiController]
[Route("api/v2/[controller]")]  // ✅ FIX: [controller] viết thường
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;  // ✅ Interface thay vì Seeder
    private readonly ICurrentUserService _currentUserService;

    public CategoriesController(ICategoryService categoryService, ICurrentUserService currentUserService)
    {
        _categoryService = categoryService;  // ✅ Inject interface
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {       
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;  // ✅ ClaimTypes imported
        
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new { message = "Truy cập bị từ chối: Không xác định được danh tính người dùng." });
        }

        var result = await _categoryService.CreateCategoryAsync(dto, userId);  // ✅ Service tồn tại
        return CreatedAtAction(nameof(CreateCategory), new { }, result);  // ✅ Trả 201
    }
}
```

**Commit Message:**
```
Fix: CategoriesController.cs - Sửa route [controller] lowercase, thêm missing using directives, sửa DI injection lỗi
```

---

### ✅ FIX #4: Dashboard.jsx - Sửa Endpoint Paths & Response Unwrapping

**File:** `MEMS.Frontend/src/pages/Dashboard.jsx`

```javascript
// ❌ TRƯỚC (Lines 52-67)
const [urgentRes, kpiRes, chartRes, insightsRes, historyRes] = await Promise.all([
  axiosClient.get('/api/v2/dashboard/urgent-tasks'),  // ❌ Thừa /api
  axiosClient.get(`/api/v2/dashboard/kpi-summary?year=${year}`),  // ❌ Thừa /api
  axiosClient.get(`/api/v2/dashboard/chart-data?year=${year}`),  // ❌ Thừa /api
  axiosClient.get(`/api/v2/dashboard/insights?year=${year}`),  // ❌ Thừa /api
  axiosClient.get(`/api/v2/dashboard/history?page=${historyParams.page}&limit=${historyParams.limit}`)  // ❌ Thừa /api
]);

setUrgentTasks(urgentRes.data.tasks || []);  // ❌ Double .data access
setKpiSummary(kpiRes.data || {});  // ❌ Double .data access
setChartData(chartRes.data.data || []);  // ❌ Double .data access
setInsights(insightsRes.data.insights || []);  // ❌ Double .data access
setReportHistory({
  data: historyRes.data.items || [],  // ❌ Double .data access
  total: historyRes.data.totalCount || 0
});

// ✅ SAU
const [urgentRes, kpiRes, chartRes, insightsRes, historyRes] = await Promise.all([
  axiosClient.get('/v2/dashboard/urgent-tasks'),  // ✅ Xóa /api (baseURL đã có)
  axiosClient.get(`/v2/dashboard/kpi-summary?year=${year}`),  // ✅ Xóa /api
  axiosClient.get(`/v2/dashboard/chart-data?year=${year}`),  // ✅ Xóa /api
  axiosClient.get(`/v2/dashboard/insights?year=${year}`),  // ✅ Xóa /api
  axiosClient.get(`/v2/dashboard/history?page=${historyParams.page}&limit=${historyParams.limit}`)  // ✅ Xóa /api
]);

// ✅ axiosClient interceptor unwrap response.data, nên không cần .data nữa
setUrgentTasks(urgentRes?.tasks || []);  // ✅ Direct access (already unwrapped)
setKpiSummary(urgentRes || {});  // ✅ Direct access
setChartData(chartRes?.data || []);  // ✅ Backend phải trả { data: [...] }
setInsights(insightsRes?.insights || []);  // ✅ Direct access
setReportHistory({
  data: historyRes?.items || [],  // ✅ Direct access
  total: historyRes?.totalCount || 0
});

// ⚠️ HOẶC: Fix Backend trả response consistent format:
// Backend nên trả: { data: [...], total: ... }
// Frontend code:
setChartData(chartRes?.data || []);
```

**Commit Message:**
```
Fix: Dashboard.jsx - Sửa endpoint paths (xóa /api prefix), sửa double .data unwrapping bug từ axios interceptor
```

---

### ✅ FIX #5: AdminPage.jsx - Sửa Backup Endpoint

**File:** `MEMS.Frontend/src/pages/AdminPage.jsx`

```javascript
// ❌ TRƯỚC (Line 21)
const response = await axiosClient.post('/admin/backup/trigger', {}, { responseType: 'blob' });

// ✅ SAU
const response = await axiosClient.post('/admin/backup', {}, { responseType: 'blob' });  // ✅ Endpoint đúng
```

**Commit Message:**
```
Fix: AdminPage.jsx - Sửa endpoint backup từ /admin/backup/trigger → /admin/backup
```

---

### ✅ FIX #6: useAuthStore.js - Lưu fullName & Strict Token Expiry

**File:** `MEMS.Frontend/src/store/useAuthStore.js`

```javascript
// ❌ TRƯỚC (Lines 17-22)
user: {
    userId: decoded.sub || decoded.nameid,
    role: decoded.role || decoded.Role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
    branchId: decoded.branchId || decoded.BranchId || decoded.branchid
},

// ✅ SAU
user: {
    userId: decoded.sub || decoded.nameid,
    fullName: decoded.fullName || decoded.FullName || 'Người dùng',  // ✅ Add
    role: decoded.role || decoded.Role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
    branchId: decoded.branchId || decoded.BranchId || decoded.branchid
},
```

**Thêm vào loadFromToken (Line 38-46):**
```javascript
// ✅ Kiểm tra token hết hạn NGAY TẠI setCredentials
setCredentials: (token) => {
  try {
    const decoded = jwtDecode(token);
    
    // ✅ Check expiration ngay
    const expiresAt = decoded.exp * 1000;
    if (expiresAt < Date.now()) {
      console.warn("Token đã hết hạn");
      set({ token: null, user: null, isAuthenticated: false });
      return;
    }
    
    set({ 
      token: token, 
      user: {
          userId: decoded.sub || decoded.nameid,
          fullName: decoded.fullName || decoded.FullName || 'Người dùng',  // ✅ Add
          role: decoded.role || decoded.Role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
          branchId: decoded.branchId || decoded.BranchId || decoded.branchid
      }, 
      isAuthenticated: true 
    });
  } catch (error) {
    console.error("Token không hợp lệ", error);
    set({ token: null, user: null, isAuthenticated: false });
  }
},
```

**Commit Message:**
```
Fix: useAuthStore.js - Lưu fullName từ JWT, thêm strict token expiration check trong setCredentials
```

---

### ✅ FIX #7: axiosClient.js - Cải Thiện Error Handling

**File:** `MEMS.Frontend/src/api/axiosClient.js`

```javascript
// ❌ TRƯỚC (Lines 30-42)
(error) => {
    if (error.response) {
        const status = error.response.status;
        if (status === 401 || status === 403) {
            useAuthStore.getState().logout();
            window.location.href = '/login';
        }
    }
    return Promise.reject(error);
},

// ✅ SAU
(error) => {
    if (error.response) {
        const status = error.response.status;
        const data = error.response.data;  // ✅ Extract error details
        
        if (status === 401 || status === 403) {
            useAuthStore.getState().logout();
            window.location.href = '/login';
        } else if (status === 400) {
            // ✅ Handle validation errors
            console.error("Validation Error:", data?.message || data?.error);
        } else if (status === 500) {
            // ✅ Handle server errors
            console.error("Server Error:", data?.message || "Lỗi hệ thống");
        }
    } else if (error.request) {
        // ✅ Request made but no response
        console.error("Network Error: Không thể kết nối đến server");
    } else {
        // ✅ Request setup error
        console.error("Request Error:", error.message);
    }
    
    // ✅ Return error object with details
    return Promise.reject({
        status: error.response?.status,
        message: error.response?.data?.message || error.message,
        error: error
    });
},
```

**Commit Message:**
```
Fix: axiosClient.js - Cải thiện error handling, extract error messages, log chi tiết
```

---

### ✅ FIX #8: Create .env.example & Documentation

**File:** `MEMS.Frontend/.env.example`

```bash
# Backend API Configuration
VITE_API_BASE_URL=http://localhost:5000/api

# Feature Flags (Optional)
VITE_DEBUG_MODE=false
```

**File:** `MEMS.Frontend/.env.development` (Local)

```bash
VITE_API_BASE_URL=http://localhost:5000/api
VITE_DEBUG_MODE=true
```

**File:** `MEMS.Frontend/.env.production` (Production)

```bash
VITE_API_BASE_URL=https://api.yourdomain.com/api
VITE_DEBUG_MODE=false
```

**Commit Message:**
```
Feat: Thêm .env configuration files và .env.example
```

---

### ✅ FIX #9: ReportsController - Standardize Response Format

**File:** `MEMS.Backend/Presentation/Controllers/ReportsController.cs`

```csharp
// ⚠️ RECOMMENDATION: Tạo Standard Response Wrapper

// Tạo file: MEMS.Backend/Presentation/DTOs/ApiResponse.cs
namespace MEMS.Backend.Presentation.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    
    public static ApiResponse<T> Ok(T data, string message = "Thành công")
        => new() { Success = true, Data = data, Message = message };
    
    public static ApiResponse<T> Error(string message)
        => new() { Success = false, Message = message };
}

// Update ReportsController để trả consistent response:
[HttpGet]
public async Task<IActionResult> GetReports(
    [FromQuery] int year,
    [FromQuery] int? month = null)
{
    try
    {
        var reports = await _reportService.GetReportsByYearMonthAsync(year, month);
        return Ok(ApiResponse<List<ReportDto>>.Ok(reports));  // ✅ Standard format
    }
    catch (Exception ex)
    {
        return BadRequest(ApiResponse<object>.Error(ex.Message));  // ✅ Standard format
    }
}

[HttpPost("{id}/approve")]
[Authorize(Roles = "SuperAdmin,Manager")]
public async Task<IActionResult> Approve(Guid id)
{
    try
    {
        var result = await _reportService.ApproveReportAsync(id);
        return Ok(ApiResponse<ApproveReportResponse>.Ok(result, "Phê duyệt báo cáo thành công"));  // ✅ 200 OK + Standard format
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(ApiResponse<object>.Error(ex.Message));  // ✅ 404
    }
    catch (Exception ex)
    {
        return StatusCode(500, ApiResponse<object>.Error("Lỗi hệ thống: " + ex.Message));  // ✅ 500
    }
}

[HttpPost("upload")]
public async Task<IActionResult> Upload(
    [FromForm] string title,
    [FromForm] int month,
    [FromForm] int year,
    IFormFile file)
{
    if (file == null || file.Length == 0)
        return BadRequest(ApiResponse<object>.Error("File không hợp lệ."));
    
    try
    {
        var result = await _reportService.UploadReportAsync(title, month, year, file);
        return CreatedAtAction(nameof(GetReportById), new { id = result.ReportId }, 
            ApiResponse<UploadReportResponse>.Ok(result, "Upload báo cáo thành công"));  // ✅ 201 Created
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(ApiResponse<object>.Error(ex.Message));
    }
    catch (Exception ex)
    {
        return StatusCode(500, ApiResponse<object>.Error("Lỗi hệ thống: " + ex.Message));
    }
}
```

**Commit Message:**
```
Feat: Thêm ApiResponse<T> wrapper, standardize tất cả ReportsController responses
```

---

### ✅ FIX #10: Frontend Dashboard - Fix Response Parsing

**File:** `MEMS.Frontend/src/pages/Dashboard.jsx` (Updated based on Backend ApiResponse)

```javascript
// Nếu Backend đổi sang ApiResponse<T>:
const fetchDashboardData = async () => {
  setLoading(true);
  const year = selectedYear.year();
  
  try {
    const [urgentRes, kpiRes, chartRes, insightsRes, historyRes] = await Promise.all([
      axiosClient.get('/v2/dashboard/urgent-tasks'),
      axiosClient.get(`/v2/dashboard/kpi-summary?year=${year}`),
      axiosClient.get(`/v2/dashboard/chart-data?year=${year}`),
      axiosClient.get(`/v2/dashboard/insights?year=${year}`),
      axiosClient.get(`/v2/dashboard/history?page=${historyParams.page}&limit=${historyParams.limit}`)
    ]);

    // ✅ Backend đang trả ApiResponse, nên response.data chứa data thực
    // axiosClient interceptor unwrap response.data, nên:
    // urgentRes = { success, message, data: { tasks: [...] } }
    
    setUrgentTasks(urgentRes?.data?.tasks || []);
    setKpiSummary(kpiRes?.data || {});
    setChartData(chartRes?.data?.data || []);
    setInsights(insightsRes?.data?.insights || []);
    setReportHistory({
      data: historyRes?.data?.items || [],
      total: historyRes?.data?.totalCount || 0
    });
  } catch (error) {
    console.error("Lỗi khi tải dữ liệu Dashboard:", error);
    message.error("Không thể tải dữ liệu dashboard");
  } finally {
    setLoading(false);
  }
};
```

**Commit Message:**
```
Fix: Dashboard.jsx - Cập nhật response parsing để phù hợp với ApiResponse wrapper
```

---

## 📊 Tóm Tắt Các Lỗi & Trạng Thái

| # | Lỗi | Severity | Status | Commit |
|----|-----|----------|--------|--------|
| 1 | Program.cs typos (app.Service, ap) | 🔴 CRITICAL | ⏳ Ready | FIX #1 |
| 2 | AdminController syntax & DI errors | 🔴 CRITICAL | ⏳ Ready | FIX #2 |
| 3 | CategoriesController route & using | 🔴 CRITICAL | ⏳ Ready | FIX #3 |
| 4 | Dashboard endpoint paths thừa /api | 🔴 CRITICAL | ⏳ Ready | FIX #4 |
| 5 | Dashboard response unwrapping bug | 🔴 CRITICAL | ⏳ Ready | FIX #4 |
| 6 | AdminPage backup endpoint sai | 🔴 CRITICAL | ⏳ Ready | FIX #5 |
| 7 | Missing .env configuration | 🟠 HIGH | ⏳ Ready | FIX #8 |
| 8 | useAuthStore fullName missing | 🟠 HIGH | ⏳ Ready | FIX #6 |
| 9 | Response format inconsistent | 🟠 HIGH | ⏳ Ready | FIX #9 |
| 10 | Error handling incomplete | 🟠 HIGH | ⏳ Ready | FIX #7 |

---

## 🎯 ĐỀ XUẤT HÀNH ĐỘNG NGAY

### **Tuần 1: CRITICAL FIXES (Phải hoàn thành trước khi test)**
- [ ] FIX #1: Program.cs typos (1 giờ)
- [ ] FIX #2: AdminController errors (2 giờ)
- [ ] FIX #3: CategoriesController errors (1 giờ)
- [ ] FIX #4: Dashboard paths & unwrapping (2 giờ)
- [ ] FIX #5: AdminPage backup endpoint (30 phút)
- **Total: 6.5 giờ**

### **Tuần 2: HIGH PRIORITY FIXES**
- [ ] FIX #6: useAuthStore improvements (1 giờ)
- [ ] FIX #7: axiosClient error handling (1 giờ)
- [ ] FIX #8: .env files setup (30 phút)
- [ ] FIX #9: Response format standardization (3 giờ)
- [ ] FIX #10: Dashboard response parsing update (1 giờ)
- **Total: 6.5 giờ**

### **Deploy Checklist**
- [ ] Backend compiles successfully (`dotnet build`)
- [ ] Frontend builds successfully (`npm run build`)
- [ ] All 10 fixes applied
- [ ] Integration tests passed (auth, branch CRUD, dashboard, upload)
- [ ] CORS tested (http://localhost:5173 ↔ http://localhost:5000)
- [ ] JWT token validation tested
- [ ] Error handling tested (401, 403, 404, 500)

---

## 📚 Tài Liệu Tham Khảo

- [JWT Claims Best Practices](https://tools.ietf.org/html/rfc7519)
- [REST API Response Standardization](https://jsonapi.org/)
- [CORS Configuration .NET 8](https://learn.microsoft.com/en-us/aspnet/core/security/cors)
- [Axios Interceptors](https://axios-http.com/docs/interceptors)
- [Zustand Store Best Practices](https://github.com/pmndrs/zustand)

---

**Báo Cáo được chuẩn bị bởi:** GitHub Copilot (API Synchronization Audit)  
**Ngày:** 2026-05-13  
**Status:** ✅ READY FOR IMPLEMENTATION
