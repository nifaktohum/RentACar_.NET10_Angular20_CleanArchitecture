using Application.Behaviors;
using Application.Common.Helpers;
using Domain.Repositories;
using Domain.Entities.Roles;
using GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Permissions.Commands;

[Permission("Permissions.Deactivate")]
public record DeactivatePermissionCommand(Guid RoleId, Guid PermissionId) : IRequest<PermissionDto>;

public sealed class DeactivatePermissionCommandHandler(
    IPermissionRepository _permissionRepo,
    IUserRepository _userRepo,
    IUnitOfWork _unit,
    IRoleRepository _roleRepo
) : IRequestHandler<DeactivatePermissionCommand, PermissionDto>
{
  public async Task<PermissionDto> Handle(DeactivatePermissionCommand request, CancellationToken cancellationToken)
  {
    // 👑 1. HEDEF ROLÜN KONTROLÜ
    var targetRole = await _roleRepo.FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
    if (targetRole is null)
    {
      return new PermissionDto(false, "❌ İşlem yapılmak istenen rol sistemde bulunamadı!", 0);
    }

    // 🔥 KRİTİK: Admin rolünün yetkileri KESİNLİKLE değiştirilemez!
    if (targetRole.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
    {
      return new PermissionDto(false, "❌ Admin rolünün yetkileri değiştirilemez!", 0);
    }

    // 2. Ana İzin (Permission) Kontrolü
    var permission = await _permissionRepo.AsQueryable()
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(p => p.Id == request.PermissionId, cancellationToken);

    if (permission is null)
    {
      return new PermissionDto(false, "İzin bulunamadı!", 0);
    }

    // 3. Kod tarafında (Assembly) hala geçerli mi kontrolü
    if (!PermissionLoader.HasPermission(permission.Name))
    {
      return new PermissionDto(false, $"'{permission.Name}' izni artık kod tarafında tanımlı değil!", 0);
    }

    // 🎯 4. ARA TABLODAN KAYDI BUL VE KALDIR
    var existingLink = await _permissionRepo.AsQueryable()
        .SelectMany(p => p.PermissionRoles)
        .FirstOrDefaultAsync(pr => pr.RoleId == request.RoleId && pr.PermissionId == request.PermissionId, cancellationToken);

    if (existingLink == null)
    {
      return new PermissionDto(false, "Bu rol zaten bu izne sahip değil!", 0);
    }

    // --- 🚀 YETKİ KALDIRILIYOR (Switch KAPALI) ---
    _permissionRepo.RemovePermissionRole(existingLink, cancellationToken);

    // 5. Güvenlik Duvarı: Bu roldeki kullanıcıların SecurityStamp'lerini sarsıyoruz
    var affectedUserIds = await _userRepo
        .Where(u => u.UserRoles.Any(ur => ur.RoleId == request.RoleId))
        .Select(u => u.Id)
        .ToListAsync(cancellationToken);

    int affectedUsersCount = affectedUserIds.Count;

    if (affectedUserIds.Any())
    {
      var usersToReset = await _userRepo.Where(u => affectedUserIds.Contains(u.Id)).ToListAsync(cancellationToken);
      foreach (var user in usersToReset)
      {
        user.UpdateSecurityStamp();
      }
    }

    await _unit.SaveChangesAsync(cancellationToken);

    return new PermissionDto(true, affectedUsersCount > 0
        ? $"Yetki rolden kaldırıldı ve bu roldeki {affectedUsersCount} kullanıcının oturumu sonlandırıldı."
        : "Yetki başarıyla rolden kaldırıldı.", affectedUsersCount);
  }
}