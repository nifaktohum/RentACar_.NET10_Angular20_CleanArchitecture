using Domain.Entities.Extras;
using Domain.Entities.Extras.Enum;
using Domain.Repositories.Extras;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Extras;

public sealed class ExtraRepository : Repository<Extra, AppDbContext>, IExtraRepository
{
  private readonly AppDbContext _context;

  public ExtraRepository(AppDbContext context) : base(context)
  {
    this._context = context;
  }

  /// Belirtilen kategorideki ürün sayısını getirir.
  /// Kategori bazlı istatistikler için kullanılır.
  public async Task<int> CountByCategoryAsync(int category, CancellationToken cancellationToken = default)
  {
    return await CountAsync(e => (int)e.Category == category && e.IsActive && !e.IsDeleted, cancellationToken);
  }

  /// Kategoriye göre ekstra ürünleri getirir.
  public async Task<List<Extra>> GetByCategoryAsync(int category, CancellationToken cancellationToken = default)
  {
    return await Where(e => (int)e.Category == category && e.IsActive && !e.IsDeleted)
                  .OrderBy(e => e.DisplayOrder)
                  .ToListAsync(cancellationToken);
  }

  /// Belirtilen ID listesine göre ekstra ürünleri getirir.
  /// Toplu sorgulamalar için kullanılır (örn: sepet özeti).
  public async Task<List<Extra>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
  {
    return await Where(e => ids.Contains(e.Id) && e.IsActive && !e.IsDeleted)
                  .ToListAsync(cancellationToken);
  }

  /// Fiyat tipine göre ekstra ürünleri getirir.
  /// Günlük ücretli (Daily) veya Kiralama başı (Rental) ürünleri filtreler.
  public async Task<List<Extra>> GetByPriceTypeAsync(int priceType, CancellationToken cancellationToken = default)
  {
    return await Where(e => (int)e.PriceType == priceType && e.IsActive && !e.IsDeleted)
                  .OrderBy(e => e.DisplayOrder)
                  .ToListAsync(cancellationToken);
  }

  /// Stokta olan ekstra ürünleri getirir.
  /// StockLimit null ise sınırsız stok, 0'dan büyükse stok var demektir. 
  public async Task<List<Extra>> GetInStockAsync(CancellationToken cancellationToken = default)
  {
    return await Where(e => (e.StockLimit == null || e.StockLimit > 0) && e.IsActive && !e.IsDeleted)
                    .OrderBy(e => e.DisplayOrder)
                    .ToListAsync(cancellationToken);
  }

  /// Önerilen ekstra ürünleri getirir.
  /// Ana sayfada veya önerilen ürünler bölümünde kullanılır.
  public async Task<List<Extra>> GetRecommendedAsync(CancellationToken cancellationToken = default)
  {
    return await Where(e => e.IsRecommended && e.IsActive && !e.IsDeleted)
                  .OrderBy(e => e.DisplayOrder)
                  .ToListAsync(cancellationToken);
  }

  /// Ekstra ürünün benzersiz olup olmadığını kontrol eder (isim bazında).
  /// Yeni ürün eklerken ve güncellerken isim çakışmasını önler.
  public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
  {
    var query = Where(e => e.Name == name && !e.IsDeleted);

    if (excludeId.HasValue)
    {
      query = query.Where(e => e.Id != excludeId.Value);
    }

    return !await query.AnyAsync(cancellationToken);
  }
}
