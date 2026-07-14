using Domain.Entities.Protection;
using Domain.Protection;
using GenericRepository;

namespace Domain.Repositories.Protection;

// ✅ BENEFIT REPOSITORY!
public interface IProtectionBenefitRepository : IRepository<ProtectionBenefit>
{
  void Attach(object entity);
  // ID'ye göre tekil getirme
  Task<ProtectionBenefit?> GetByIdBenefitsAsync(Guid id, CancellationToken cancellationToken = default);
  // Kategoriye göre listeleme
  Task<List<ProtectionBenefit>> GetBenefitsByCategoryAsync(BenefitCategory category, CancellationToken cancellationToken = default);
  // Tüm aktifleri listeleme
  Task<List<ProtectionBenefit>> GetAllBenefitsAsync(CancellationToken cancellationToken = default);
}
