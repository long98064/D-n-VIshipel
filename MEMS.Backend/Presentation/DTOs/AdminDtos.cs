namespace MEMS.Backend.Presentation.DTOs;

public record BranchDto(Guid Id, string Name, string Address);

public record UserDto(Guid Id, string Username, string FullName, string RoleName, Guid RoleId, Guid BranchId);

public record RoleDto(Guid Id, string Name);