using Domain.Entities.Extras;
using Domain.Repositories.Extras;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Extras;

/// Kiralama-ekstra ilişki tablosunun veritabanı işlemlerini yönetir.
public sealed class RentalExtraRepository : Repository<RentalExtra, AppDbContext>, IRentalExtraRepository
{
  private readonly AppDbContext _context;

  public RentalExtraRepository(AppDbContext context) : base(context)
  {
    this._context = context;
  }

  /// Belirtilen kiralama ID'sine ait tüm ekstraları soft delete ile siler.
  /// Kiralama iptal edildiğinde veya tamamlandığında tüm ekstralar temizlenir.
  public async Task DeleteByRentalIdAsync(Guid rentalId, Guid userId, CancellationToken cancellationToken = default)
  {
    var rentalExtras = await Where(re => re.RentalId == rentalId && re.IsActive && !re.IsDeleted)
            .ToListAsync(cancellationToken);

    foreach (var rentalExtra in rentalExtras)
    {
      // Soft delete işlemi
      rentalExtra.SoftDelete(userId);
      Update(rentalExtra);
    }
  }

  /// Belirtilen ekstra ürün ID'sine ait tüm kiralamaları getirir.
  /// Raporlama ve analiz için kullanılır.
  public async Task<List<RentalExtra>> GetByExtraIdAsync(Guid extraId, CancellationToken cancellationToken = default)
  {
    return await Where(re => re.ExtraId == extraId && re.IsActive && !re.IsDeleted)
           .ToListAsync(cancellationToken);
  }

  /// Belirtilen kiralama ve ekstra ürün kombinasyonunu getirir.
  /// Aynı ürünün tekrar eklenmesini önlemek için kontrol amaçlı kullanılır.
  public async Task<RentalExtra?> GetByRentalAndExtraAsync(Guid rentalId, Guid extraId, CancellationToken cancellationToken = default)
  {
    return await FirstOrDefaultAsync(
          re => re.RentalId == rentalId
                && re.ExtraId == extraId
                && re.IsActive
                && !re.IsDeleted,
          cancellationToken,
          isTrackingActive: false
      );
  }

  /// Belirtilen kiralama ID'sine ait tüm ekstraları getirir.
  /// Sadece aktif (IsActive = true, IsDeleted = false) olanları döndürür.
  public async Task<List<RentalExtra>> GetByRentalIdAsync(Guid rentalId, CancellationToken cancellationToken = default)
  {
    return await Where(re => re.RentalId == rentalId && re.IsActive && !re.IsDeleted)
                  .ToListAsync(cancellationToken);
  }

  /// Belirtilen kiralama ID'sine ait ekstraları, ürün bilgileriyle birlikte getirir.
  /// Include ile Extra tablosunu da join'ler.
  /// Kiralama detay sayfasında ürün isimleriyle birlikte göstermek için kullanılır.
  public async Task<List<RentalExtra>> GetByRentalIdWithExtraAsync(Guid rentalId, CancellationToken cancellationToken = default)
  {
    // GenericRepository'de Include desteği yoksa Context üzerinden yapıyoruz
    return await _context.Set<RentalExtra>()
        .Include(re => re.Extra)  // Extra bilgilerini de getir
        .Where(re => re.RentalId == rentalId && re.IsActive && !re.IsDeleted)
        .ToListAsync(cancellationToken);
  }

  /// Belirtilen kiralama ID'sine ait ekstraların toplam fiyatını getirir.
  /// Özet fiyat hesaplamalarında kullanılır.
  public async Task<decimal> GetTotalPriceByRentalIdAsync(Guid rentalId, CancellationToken cancellationToken = default)
  {
    return await Where(re => re.RentalId == rentalId && re.IsActive && !re.IsDeleted)
          .SumAsync(re => re.TotalPrice, cancellationToken);
  }
}
