using Application.Behaviors;
using Application.Common.Helpers;
using Domain.Repositories;
using Domain.Roles; // PermissionRole entity'si için kanks
using GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Permissions.Commands;

[Permission("Permissions.Activate")]
public record ActivatePermissionCommand(Guid RoleId, Guid PermissionId) : IRequest<PermissionDto>;

public sealed class ActivatePermissionCommandHandler(
    IPermissionRepository _permissionRepo,
    IUserRepository _userRepo,
    IUnitOfWork _unit,
    IRoleRepository _roleRepo
) : IRequestHandler<ActivatePermissionCommand, PermissionDto>
{
  public async Task<PermissionDto> Handle(ActivatePermissionCommand request, CancellationToken cancellationToken)
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

    // 2. Ana İzin (Permission) Kontrolü (Sistemde böyle bir izin var mı?)
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

    // 🎯 4. ESNEK MİMARİ KONTROLÜ JOE:
    // Bu rol ile bu izin arasında zaten bir bağ (kayıt) var mı diye kontrol ediyoruz
    // _permissionRepo.PermissionRoles (veya senin context yapına göre) ara tabloya sorgu atıyoruz
    var existingLink = await _permissionRepo.AsQueryable()
        .SelectMany(p => p.PermissionRoles)
        .FirstOrDefaultAsync(pr => pr.RoleId == request.RoleId && pr.PermissionId == request.PermissionId, cancellationToken);

    if (existingLink != null)
    {
      return new PermissionDto(true, "Bu rol zaten bu izne sahip!", 0);
    }


    // --- 🚀 YETKİ ATANIYOR (Switch AÇIK) ---
    var newPermissionRole = new PermissionRole(request.RoleId, request.PermissionId);
    await _permissionRepo.AddPermissionRoleAsync( newPermissionRole, cancellationToken);

    // 5. Güvenlik Duvarı: Bu roldeki kullanıcıların SecurityStamp'lerini sarsıyoruz Joe
    // Rolün yetkisi değiştiği için o roldeki tüm kullanıcıları bulup token'larını düşüreceğiz kanks
    var affectedUserIds = await _userRepo.Where(u => u.UserRoles.Any(ur => ur.RoleId == request.RoleId))
                                         .Select(u => u.Id)
                                         .ToListAsync(cancellationToken);

    int affectedUsersCount = affectedUserIds.Count;

    if (affectedUserIds.Any())
    {
      var usersToReset = await _userRepo.Where(u => affectedUserIds.Contains(u.Id)).ToListAsync(cancellationToken);
      foreach (var user in usersToReset)
      {
        user.UpdateSecurityStamp(); // Bir sonraki isteklerinde token'ları yenilensin diye Joe!
      }
    }

    // Tüm değişiklikleri (Ara tablo Insert + SecurityStamp Update) tek transaction'da uçuruyoruz
    await _unit.SaveChangesAsync(cancellationToken);

    return new PermissionDto(true, affectedUsersCount > 0
        ? $"İzin başarıyla role atandı ve bu roldeki {affectedUsersCount} kullanıcının yetkileri güncellendi."
        : "İzin başarıyla role atandı.", affectedUsersCount);
  }
}