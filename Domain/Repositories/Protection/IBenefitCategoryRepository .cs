using Domain.Entities.Protection;
using GenericRepository;

namespace Domain.Repositories.Protection;

public interface IBenefitCategoryRepository: IRepository<BenefitCategory>
{
  /// Aktif olan tüm kategorileri getirir (IsActive = true ve IsDeleted = false)
  Task<List<BenefitCategory>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
  /// Slug'a göre kategori getirir
  Task<BenefitCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
  /// Kategori ve altındaki Benefit'leri birlikte getirir
  Task<BenefitCategory?> GetCategoryWithBenefitsAsync(Guid id, CancellationToken cancellationToken = default);
}
