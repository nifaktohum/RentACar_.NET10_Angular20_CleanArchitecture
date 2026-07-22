using Domain.Entities.Protection;
using GenericRepository;

namespace Domain.Repositories.Protection;

public interface IProtectionPackageRepository : IRepository<ProtectionPackage>
{
  void Attach(object entity);
  IQueryable<ProtectionPackage> GetAll(bool ignoreFilters = false);
  Task<List<ProtectionPackage>> GetActivePackagesAsync(CancellationToken cancellationToken = default);
  Task<ProtectionPackage?> GetPackageWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
  Task<List<ProtectionPackage>> GetRecommendedPackagesAsync(CancellationToken cancellationToken = default);
}

