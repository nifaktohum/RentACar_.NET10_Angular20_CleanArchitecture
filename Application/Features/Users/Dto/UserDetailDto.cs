public record UserDetailDto(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? BranchName,
    Guid BranchId,  // Nullable DEĞİL!
    bool IsActive,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    string CreatedByName,
    DateTimeOffset? UpdatedAt,
    Guid UpdatedBy,
    string? UpdatedByName,
    List<UserRoleDto> Roles,
    List<UserPermissionDto> Permissions
);

public record UserRoleDto(
    Guid RoleId,
    string RoleName,
    string? Description
);

public record UserPermissionDto(
    Guid PermissionId,
    string PermissionName,
    string? Module
);