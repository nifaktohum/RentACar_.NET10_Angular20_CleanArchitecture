using Application.Common.Helpers;
using Domain.Roles;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions;

// Rol oluşturma, koddan izinleri basma, şalterleri tamir etme ve Admin'e köprü kurma işleri tamamen burada.
public static class RoleAndPermissionSeed
{
  public static async Task RolesAndPermissionsAsync(this AppDbContext context, IConfiguration config)
  {
    Guid adminRoleId = Guid.Parse(config["SeedData:AdminRoleId"]!);
    Guid customerRoleId = Guid.Parse(config["SeedData:CustomerRoleId"]!);

    // 1. Statik Rolleri Çakıyoruz (Customer boş doğuyor)
    var staticRolesToCheck = new[]
    {
            new { Id = adminRoleId, Name = "Admin", Description = "Sistemdeki tüm yetkilere sahip en üst düzey yönetici rolü." },
            new { Id = customerRoleId, Name = "Customer", Description = "Araç kiralama yapabilen standart mobil ve web müşterisi." }
        };

    bool rolesChanged = false;
    foreach (var staticRole in staticRolesToCheck)
    {
      var anyRole = await context.Roles.IgnoreQueryFilters().AnyAsync(r => r.Id == staticRole.Id);
      if (!anyRole)
      {
        var newRole = new Role(staticRole.Name, staticRole.Description);
        context.Entry(newRole).Property(x => x.Id).CurrentValue = staticRole.Id;
        await context.Roles.AddAsync(newRole);
        rolesChanged = true;
      }
    }
    if (rolesChanged) await context.SaveChangesAsync();

    // 2. Koddan İzinleri Basıyoruz
    var currentPermissionsInCode = PermissionLoader.GetAllPermissions();
    var existingPermissions = await context.Permissions.IgnoreQueryFilters().Select(p => p.Name).ToListAsync();
    bool permissionsChanged = false;

    foreach (var permissionName in currentPermissionsInCode)
    {
      if (!existingPermissions.Contains(permissionName))
      {
        var newPermission = new Permission(permissionName, $"{permissionName} eylemini yapma yetkisi.");
        await context.Permissions.AddAsync(newPermission);
        permissionsChanged = true;
      }
    }
    if (permissionsChanged) await context.SaveChangesAsync();

    // 3. Sadece Admin'e Tüm Yetkileri Bağlıyoruz (Zero Trust Duvarı)
    var existingAdminPermissionIds = await context.PermissionRoles
        .IgnoreQueryFilters()
        .Where(pr => pr.RoleId == adminRoleId)
        .Select(pr => pr.PermissionId)
        .ToListAsync();

    var allPermissionsInDb = await context.Permissions.IgnoreQueryFilters().AsNoTracking().ToListAsync();
    bool adminBridgeChanged = false;

    foreach (var permission in allPermissionsInDb)
    {
      if (!existingAdminPermissionIds.Contains(permission.Id))
      {
        var permissionRoleBridge = new PermissionRole(adminRoleId, permission.Id);
        await context.PermissionRoles.AddAsync(permissionRoleBridge);
        adminBridgeChanged = true;
      }
    }
    if (adminBridgeChanged) await context.SaveChangesAsync();
  }

}