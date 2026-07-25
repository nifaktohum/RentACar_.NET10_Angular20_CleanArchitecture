using Domain.Entities.Extras;
using GenericRepository;

namespace Domain.Repositories.Extras;

public interface IRentalExtraRepository: IRepository<RentalExtra>
{

  /// Belirtilen kiralama ID'sine ait tüm ekstraları getirir.
  Task<List<RentalExtra>> GetByRentalIdAsync(Guid rentalId, CancellationToken cancellationToken = default);

  /// Belirtilen ekstra ürün ID'sine ait tüm kiralamaları getirir.
  /// (Hangi kiralamalarda bu ürün kullanılmış? - Raporlama için)
  Task<List<RentalExtra>> GetByExtraIdAsync(Guid extraId, CancellationToken cancellationToken = default);

  /// Belirtilen kiralama ve ekstra ürün kombinasyonunu getirir.
  /// (Aynı ürün tekrar eklenmesin diye kontrol için)
  Task<RentalExtra?> GetByRentalAndExtraAsync(Guid rentalId, Guid extraId, CancellationToken cancellationToken = default);

  /// Belirtilen kiralama ID'sine ait ekstraları, ürün bilgileriyle birlikte getirir.
  /// (Include ile Extra tablosunu da join'ler)
  Task<List<RentalExtra>> GetByRentalIdWithExtraAsync(Guid rentalId, CancellationToken cancellationToken = default);

  /// Belirtilen kiralama ID'sine ait tüm ekstraları soft delete ile siler.
  Task DeleteByRentalIdAsync(Guid rentalId, Guid userId, CancellationToken cancellationToken = default);

  /// Belirtilen kiralama ID'sine ait ekstraların toplam fiyatını getirir.
  Task<decimal> GetTotalPriceByRentalIdAsync(Guid rentalId, CancellationToken cancellationToken = default);
}
