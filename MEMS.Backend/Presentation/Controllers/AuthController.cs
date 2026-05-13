using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MEMS.Backend.Presentation.DTOs;
using MEMS.Backend.Infrastructure.Data;

namespace MEMS.Backend.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _dbContext;

    public AuthController(IConfiguration config, AppDbContext dbContext)
    {
        _config = config;
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. Tìm User từ DB theo Username (bỏ qua Global Query Filter để cho phép tìm)
        var user = await _dbContext.Users
            .IgnoreQueryFilters() // Bypass Global Filter (IsDeleted, BranchId)
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Username == request.Username && !u.IsDeleted);

        if (user == null)
        {
            return Unauthorized("Tài khoản hoặc mật khẩu không đúng.");
        }

        // 2. Verify Password bằng BCrypt (PHẢI ĐỒNG BỘ với hàm hash ở Seed Data)
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return Unauthorized("Tài khoản hoặc mật khẩu không đúng.");
        }

        // 3. Tạo JWT Claims từ dữ liệu thực
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("role", user.Role?.Name ?? "User"),
            new Claim("branchId", user.BranchId.ToString()),
            new Claim("fullName", user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // 4. Ký JWT Token
        var keyStr = _config["Jwt:Key"] ?? "MEMS.V2.0.SuperSecretKey.Length.Must.Be.At.Least.32.Bytes.Long.For.HMACSHA256";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "MEMS.Backend",
            audience: _config["Jwt:Audience"] ?? "MEMS.Frontend",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return Ok(new LoginResponse 
        { 
            Token = new JwtSecurityTokenHandler().WriteToken(token) 
        });
    }
}
