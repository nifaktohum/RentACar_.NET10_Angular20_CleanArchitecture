using Domain.Repositories;
using Domain.Roles;
using Domain.Users;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class PermissionRepository : Repository<Permission, AppDbContext>, IPermissionRepository
{
  private AppDbContext _context;
  public PermissionRepository(AppDbContext context) : base(context)
  {
    _context = context;
  }

  public async Task AddPermissionRoleAsync( PermissionRole permissionRole, CancellationToken cancellationToken)
  {
    var permission = await _context.Permissions
        .Include(p => p.PermissionRoles)
        .FirstOrDefaultAsync(
            p => p.Id == permissionRole.PermissionId,
            cancellationToken);

    var role = await _context.Roles
        .Include(r => r.PermissionRoles)
        .FirstOrDefaultAsync(
            r => r.Id == permissionRole.RoleId,
            cancellationToken);

    if (permission != null && role != null)
    {
      permission.AddRoleLink(permissionRole);
      role.AddPermissionLink(permissionRole);

      await _context.PermissionRoles.AddAsync(
          permissionRole,
          cancellationToken);

      Console.WriteLine(
          $"PermissionRole State After AddAsync = {_context.Entry(permissionRole).State}"
      );
    }
  }

  public void RemovePermissionRole(PermissionRole permissionRole, CancellationToken cancellationToken)
  {
    // 🔍 1. İlgili permission ve role'leri ilişkileriyle birlikte çekiyoruz kanks
    var permission = _context.Permissions
        .Include(p => p.PermissionRoles)
        .IgnoreQueryFilters() // Soft-delete varsa takılmasın Joe
        .FirstOrDefault(p => p.Id == permissionRole.PermissionId);

    var role = _context.Roles
        .Include(r => r.PermissionRoles)
        .FirstOrDefault(r => r.Id == permissionRole.RoleId);

    if (permission != null && role != null)
    {
      // 🎯 SIHİRLİ DOKUNUŞ: Dışarıdan gelen parametre yerine, 
      // Veritabanından gelen listedeki GERÇEK CANLI NESNEYİ (referansı) buluyoruz!
      var dbPermissionRole = permission.PermissionRoles
          .FirstOrDefault(pr => pr.RoleId == permissionRole.RoleId && pr.PermissionId == permissionRole.PermissionId);

      if (dbPermissionRole != null)
      {
        // Artık C# bellekte bu nesneyi şak diye bulacak ve listeden silecek Joe!
        permission.RemoveRoleLink(dbPermissionRole);
        role.RemovePermissionLink(dbPermissionRole);

        // Ekstra Güvence: EF Core'a bu ara tablo nesnesinin tamamen silineceğini (State: Deleted) açıkça bildiriyoruz
        _context.PermissionRoles.Remove(dbPermissionRole);
      }
    }
  }

  public async Task<List<Guid>> GetUserIdsByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken)
  {
    // 🔥 DbContext'in tüm gücüyle ilişkisel sorguyu burada atıyoruz Joe!
    // Bu permission'a sahip olan roller
    var roleIds = await _context.PermissionRoles
        .Where(pr => pr.PermissionId == permissionId)
        .Select(pr => pr.RoleId)
        .Distinct()
        .ToListAsync(cancellationToken);

    if (!roleIds.Any()) return new List<Guid>();

    // Bu rollere sahip olan kullanıcılar
    return await _context.UserRoles
        .Where(ur => roleIds.Contains(ur.RoleId))
        .Select(ur => ur.UserId)
        .Distinct()
        .ToListAsync(cancellationToken);
  }

}
