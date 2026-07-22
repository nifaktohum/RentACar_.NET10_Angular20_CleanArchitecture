using Application.Common.Helpers;
using Domain.Entities.Roles;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions;

public static class AdminPermissionsActive
{
  // 🔌 Sadece Admin'in cüzdanını kontrol eden, eksik switch'leri AÇIK (true) yapan meşhur sigorta metodu!
  public static async Task AdminPermissionsActiveAsync(this AppDbContext context, IConfiguration config)
  {
    Guid adminRoleId = Guid.Parse(config["SeedData:AdminRoleId"]!);
    var currentPermissionsInCode = PermissionLoader.GetAllPermissions();

    // 1. Veritabanındaki aktif olan tüm izin şablonlarını alıyoruz
    var allPermissionsInDb = await context.Permissions
        .IgnoreQueryFilters()
        .Where(p => currentPermissionsInCode.Contains(p.Name) && p.IsActive && !p.IsDeleted)
        .ToListAsync();

    // 2. Admin rolünün şu an sahip olduğu mevcut açık switch'leri (köprüleri) buluyoruz
    var existingAdminPermissionIds = await context.PermissionRoles
        .IgnoreQueryFilters()
        .Where(pr => pr.RoleId == adminRoleId)
        .Select(pr => pr.PermissionId)
        .ToListAsync();

    bool bridgeChanged = false;

    // 3. Kodda olan ama Admin'de olmayan eksik yetkileri tarıyoruz
    foreach (var permission in allPermissionsInDb)
    {
      // Eğer yeni eklenen bir izin kodda var ama Admin'in cüzdanında eksikse (switch kapalıysa):
      if (!existingAdminPermissionIds.Contains(permission.Id))
      {
        // Köprüyü kurup Admin için o switch'i şak diye açıyoruz!
        var permissionRoleBridge = new PermissionRole(adminRoleId, permission.Id);
        await context.PermissionRoles.AddAsync(permissionRoleBridge);
        bridgeChanged = true;
      }
    }

    if (bridgeChanged)
    {
      await context.SaveChangesAsync();
      Console.WriteLine("👑 [AdminPermissionsActive Extension] Admin rolünün tüm eksik veya yeni gelen yetki switch'leri başarıyla AÇILDI!");
    }
  }
}
