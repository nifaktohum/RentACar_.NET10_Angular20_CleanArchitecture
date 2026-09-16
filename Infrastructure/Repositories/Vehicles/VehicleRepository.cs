using System.Linq.Expressions;
using Domain.Entities.Vehicles;
using Domain.Repositories.Vehicles;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Vehicles;

public sealed class VehicleRepository : Repository<Vehicle, AppDbContext>, IVehicleRepository
{
  private readonly AppDbContext _context;

  public VehicleRepository(AppDbContext context) : base(context)
  {
    this._context = context;
  }

  // ==================== ÖZEL SORGULAR ====================

  // Belirtilen ID'ye sahip aracı; türü ve görselleri gibi tüm detay ilişkileriyle birlikte getirir.
  public async Task<Vehicle?> GetVehicleWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.Vehicles
        .Where(v => v.Id == id && !v.IsDeleted)
        .Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType)
        .Include(v => v.Images.Where(i => !i.IsDeleted))
        .FirstOrDefaultAsync(cancellationToken);
  }

  // Belirtilen ID'ye sahip aracı, ilişkili görselleriyle (Images) birlikte veritabanından getirir.
  public async Task<Vehicle?> GetVehicleWithImagesAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.Vehicles
        .Where(v => v.Id == id && !v.IsDeleted)
        .Include(v => v.Images.Where(i => !i.IsDeleted))
        .FirstOrDefaultAsync(cancellationToken);
  }

  // ==================== KONTROL METODLARI ====================

  // Belirli bir ID'ye sahip araçtan istenen miktarda (quantity) stokta bulunup bulunmadığını kontrol eder.
  public async Task<bool> HasStockAsync(Guid id, int quantity = 1, CancellationToken cancellationToken = default)
  {
    var vehicle = await _context.Vehicles
        .Where(v => v.Id == id && !v.IsDeleted)
        .Select(v => new { v.VehicleModel.AvailableStock })
        .FirstOrDefaultAsync(cancellationToken);

    return vehicle != null && vehicle.AvailableStock >= quantity;
  }

  // Plakanın sistemde daha önce kayıtlı olup olmadığını kontrol eder. 
  // Güncelleme işlemlerinde mevcut aracın ID'si excludeId ile hariç tutulabilir.
  public async Task<bool> IsPlateUniqueAsync(string plate, Guid? excludeId = null, CancellationToken cancellationToken = default)
  {
    var query = _context.Vehicles
        .Where(v => !v.IsDeleted && v.Plate.ToLower() == plate.ToLower());

    if (excludeId.HasValue)
      query = query.Where(v => v.Id != excludeId.Value);

    return !await query.AnyAsync(cancellationToken);
  }

  // ==================== STOK İŞLEMLERİ ====================

  // Araç kiralandığında veya stok düştüğünde, belirtilen miktarda stok miktarını azaltır ve başarılı olup olmadığını döner.
  public async Task<bool> DecreaseStockAsync(Guid id, int quantity = 1, CancellationToken cancellationToken = default)
  {
    var affected = await _context.Vehicles
        .Where(v => v.Id == id && !v.IsDeleted && v.VehicleModel.AvailableStock >= quantity)
        .ExecuteUpdateAsync(
            setter => setter
                .SetProperty(v => v.VehicleModel.AvailableStock, v => v.VehicleModel.AvailableStock - quantity)
                .SetProperty(v => v.IsAvailable, v => v.VehicleModel.AvailableStock - quantity > 0),
            cancellationToken
        );

    return affected > 0;
  }

  // İptal veya iade durumlarında, belirtilen miktarda araç stok miktarını artırır.
  public async Task<bool> IncreaseStockAsync(Guid id, int quantity = 1, CancellationToken cancellationToken = default)
  {
    var affected = await _context.Vehicles
        .Where(v => v.Id == id && !v.IsDeleted)
        .ExecuteUpdateAsync(
            setter => setter
                .SetProperty(v => v.VehicleModel.AvailableStock, v => v.VehicleModel.AvailableStock + quantity)
                .SetProperty(v => v.IsAvailable, true),
            cancellationToken
        );

    return affected > 0;
  }

  // ==================== FİLTRELEME ====================

  // Verilen filtre kriterlerine uyan toplam araç sayısını (sayfalama yapmadan önce) döner.
  public async Task<int> GetFilteredCountAsync(
         Expression<Func<Vehicle, bool>>? filter = null,
         CancellationToken cancellationToken = default)
  {
    var query = _context.Vehicles.Where(v => !v.IsDeleted);

    if (filter != null)
      query = query.Where(filter);

    return await query.CountAsync(cancellationToken);
  }

  // Dinamik filtreleme, sıralama (sorting) ve sayfalama (pagination - skip/take) kriterlerine göre araç listesini getirir.
  public async Task<List<Vehicle>> GetFilteredAsync(
         Expression<Func<Vehicle, bool>>? filter = null,
         string? sortBy = null,
         bool descending = false,
         int? skip = null,
         int? take = null,
         CancellationToken cancellationToken = default)
  {
    var query = _context.Vehicles
        .Where(v => !v.IsDeleted)
        .Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType)
        .Include(v => v.Images.Where(i => !i.IsDeleted && i.IsMain))
        .AsQueryable();

    if (filter != null)
      query = query.Where(filter);

    // Sıralama
    if (!string.IsNullOrEmpty(sortBy))
    {
      query = sortBy.ToLower() switch
      {
        "brand" => descending ? query.OrderByDescending(v => v.Brand) : query.OrderBy(v => v.Brand),
        "model" => descending ? query.OrderByDescending(v => v.Model) : query.OrderBy(v => v.Model),
        "year" => descending ? query.OrderByDescending(v => v.Year) : query.OrderBy(v => v.Year),
        "price" or "dailyprice" => descending ? query.OrderByDescending(v => v.DailyPrice) : query.OrderBy(v => v.DailyPrice),
        "stock" => descending ? query.OrderByDescending(v => v.VehicleModel.AvailableStock) : query.OrderBy(v => v.VehicleModel.AvailableStock),
        _ => descending ? query.OrderByDescending(v => v.CreatedAt) : query.OrderBy(v => v.CreatedAt)
      };
    }
    else
    {
      query = query.OrderByDescending(v => v.CreatedAt);
    }

    // Sayfalama
    if (skip.HasValue)
      query = query.Skip(skip.Value);

    if (take.HasValue)
      query = query.Take(take.Value);

    return await query.AsNoTracking().ToListAsync(cancellationToken);
  }



}
