namespace Application.Features.Permissions.Commands;

public sealed record PermissionDto(
    bool IsSuccess,
    string Message,
    int AffectedUsersCount
);

public sealed record RolePermissionQueryDto(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    bool IsSystemDisabled 
);
