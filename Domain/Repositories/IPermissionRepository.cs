using Domain.Entities.Roles;
using Domain.Entities.Users;
using GenericRepository;

namespace Domain.Repositories;

public interface IPermissionRepository : IRepository<Permission>
{
  // 🔥 İşte DbContext kullanmadan, işi soyutlayarak çözeceğimiz o şanlı metot!
  Task<List<Guid>> GetUserIdsByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken);
  Task AddPermissionRoleAsync(PermissionRole permissionRole, CancellationToken cancellationToken);
  void RemovePermissionRole(PermissionRole permissionRole, CancellationToken cancellationToken);

}