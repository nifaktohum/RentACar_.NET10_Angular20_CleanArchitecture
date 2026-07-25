using GenericRepository;
using Domain.Entities.Extras;
using Domain.Entities.Extras.Enum;

namespace Domain.Repositories.Extras;

public interface IExtraRepository: IRepository<Extra>
{

  /// Kategoriye göre ekstra ürünleri getirir.
  Task<List<Extra>> GetByCategoryAsync(int category, CancellationToken cancellationToken = default);

  /// Fiyat tipine göre ekstra ürünleri getirir.
  Task<List<Extra>> GetByPriceTypeAsync(int priceType, CancellationToken cancellationToken = default);

  /// Önerilen (isRecommended = true) ekstra ürünleri getirir.
  Task<List<Extra>> GetRecommendedAsync(CancellationToken cancellationToken = default);

  /// Stokta olan (stockLimit > 0 veya null) ekstra ürünleri getirir.
  Task<List<Extra>> GetInStockAsync(CancellationToken cancellationToken = default);

  /// Belirtilen ID listesine göre ekstra ürünleri getirir.
  Task<List<Extra>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default);

  /// Belirtilen kategorideki ürün sayısını getirir.
  Task<int> CountByCategoryAsync(int category, CancellationToken cancellationToken = default);

  /// Ekstra ürünün benzersiz olup olmadığını kontrol eder (isim bazında).
  Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
