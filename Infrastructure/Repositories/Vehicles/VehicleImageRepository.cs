using Domain.Entities.Vehicles;
using Domain.Repositories.Vehicles;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Vehicles;

public sealed class VehicleImageRepository : Repository<VehicleImage, AppDbContext>, IVehicleImageRepository
{
  private readonly AppDbContext _context;
  private readonly IWebHostEnvironment _env;

  public VehicleImageRepository(AppDbContext context, IWebHostEnvironment env) : base(context)
  {
    this._context = context;
    this._env = env;
  }




  // Belirtilen araca ait silinmemiş (aktif) olan görsellerin listesini getirir.
  public async Task<List<VehicleImage>> GetActiveImagesByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
  {
    return await _context.VehicleImages
        .Where(i => !i.IsDeleted && i.IsActive && i.VehicleId == vehicleId)
        .OrderBy(i => i.DisplayOrder)
        .AsNoTracking()
        .ToListAsync(cancellationToken);
  }

  public async Task<List<VehicleImage>> GetActiveImagesByVehicleWithTrackingAsync(Guid vehicleId, CancellationToken cancellationToken = default)
  {
    return await _context.VehicleImages
        .Where(i => !i.IsDeleted && i.IsActive && i.VehicleId == vehicleId)
        .OrderBy(i => i.DisplayOrder)
        .ToListAsync(cancellationToken); // AsNoTracking() kaldırıldı
  }

  // Belirtilen araca ait toplam görsel sayısını verir.
  public async Task<int> GetImageCountAsync(Guid vehicleId, CancellationToken cancellationToken = default)
  {
    return await _context.VehicleImages
        .CountAsync(i => !i.IsDeleted && i.IsActive && i.VehicleId == vehicleId, cancellationToken);
  }

  // Belirtilen araca ait tüm görsellerin listesini getirir.
  public async Task<List<VehicleImage>> GetImagesByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
  {
    return await _context.VehicleImages
        .Where(i => !i.IsDeleted && i.VehicleId == vehicleId)
        .OrderBy(i => i.DisplayOrder)
        .AsNoTracking()
        .ToListAsync(cancellationToken);
  }

  // Belirtilen araca ait ana görseli (IsMain = true) veritabanından getirir.
  public async Task<VehicleImage?> GetMainImageAsync(Guid vehicleId, CancellationToken cancellationToken = default)
  {
    return await _context.VehicleImages
        .Where(i => !i.IsDeleted && i.VehicleId == vehicleId && i.IsMain)
        .AsNoTracking()
        .FirstOrDefaultAsync(cancellationToken);
  }

  // Belirtilen araca ait işaretlenmiş bir ana görselin (IsMain = true) bulunup bulunmadığını kontrol eder (True/False döner).
  public async Task<bool> HasMainImageAsync(Guid vehicleId, CancellationToken cancellationToken = default)
  {
    return await _context.VehicleImages
        .AnyAsync(i => !i.IsDeleted && i.VehicleId == vehicleId && i.IsMain, cancellationToken);
  }

  // Belirtilen araca ait tüm görsellerin ana görsel olma durumunu (IsMain) kapatarak false yapar (Genellikle yeni bir ana görsel seçilmeden önce eskileri sıfırlamak için kullanılır).
  public async Task SetAllAsNonMainAsync(Guid vehicleId, CancellationToken cancellationToken = default)
  {
    await _context.VehicleImages
        .Where(i => !i.IsDeleted &&
                    i.VehicleId == vehicleId &&
                    i.IsMain)
        .ExecuteUpdateAsync(
            setter => setter.SetProperty(i => i.IsMain, false),
            cancellationToken
        );
  }


  public async Task AddVehicleImageAsync(Vehicle vehicle, string imageUrl, bool isMain, string? description, CancellationToken ct)
  {
    vehicle.AddImage(imageUrl, isMain, description); // domain nesnesini ve iş kurallarını yine bu oluşturur

    var newImage = vehicle.Images.Last(); // az önce eklenen resim
    _context.VehicleImages.Add(newImage);  // <-- EF'i bunu Added olarak track etmeye zorlar

    await _context.SaveChangesAsync(ct);
  }

  //  Dosya Silme Metodu
  public void DeletePhysicalFile(string imageUrl)
  {
    if (string.IsNullOrEmpty(imageUrl)) return;

    var filePath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
    if (File.Exists(filePath))
      File.Delete(filePath);
  }

}

/*  ExecuteUpdateAsync() ==>
          veritabanındaki kayıtları tek tek memory'e (belleğe) çekmeden, 
          doğrudan SQL katmanında toplu olarak güncellememizi (bulk update) sağlayan çok güçlü bir metottur.

    SetProperty() ==> 
          veritabanında güncellenecek kolonları ve verilecek yeni değerleri SQL seviyesinde belirtmemizi sağlayan bir lambdasıdır.
          Hangi kolonun hangi değerle güncelleneceğini tipi güvenli (type-safe) bir şekilde ifade etmemize olanak tanır.
*/
