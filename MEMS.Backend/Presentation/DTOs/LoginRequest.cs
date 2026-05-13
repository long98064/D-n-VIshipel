namespace MEMS.Backend.Presentation.DTOs;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    // Tuyệt đối không nhận tham số BranchId ở đây!
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}
