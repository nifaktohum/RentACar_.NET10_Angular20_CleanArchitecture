using Domain.Entities.Protection;
using GenericRepository;

namespace Domain.Repositories.Protection;

// ✅ BENEFIT REPOSITORY!
public interface IProtectionBenefitRepository : IRepository<ProtectionBenefit>
{
  void Attach(object entity);
  /// Kategori ID'sine göre benefit'leri getirir (Entity!)
  Task<ProtectionBenefit?> GetByIdBenefitAsync(Guid id, CancellationToken cancellationToken = default);
  Task<List<ProtectionBenefit>> GetBenefitsByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
  // Kategoriye göre listeleme
  Task<List<ProtectionBenefit>> GetBenefitsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
  // Tüm aktifleri listeleme
  Task<List<ProtectionBenefit>> GetAllBenefitsAsync(CancellationToken cancellationToken = default);
  /// Aktif benefit'leri getirir
  Task<List<ProtectionBenefit>> GetActiveBenefitsAsync(CancellationToken cancellationToken = default);
}
