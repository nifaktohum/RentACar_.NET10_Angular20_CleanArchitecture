using Domain.Abstractions;

namespace Domain.Entities.Roles;

public sealed class PermissionRole 
{
  private PermissionRole() { }
  public PermissionRole(Guid roleId, Guid permissionId) 
  {
    RoleId = roleId;
    PermissionId = permissionId;
  }


  public Guid RoleId { get; private set; }
  public Role Role { get; private set; } = default!;

  public Guid PermissionId { get; private set; }
  public Permission Permission { get; private set; } = default!;
}
