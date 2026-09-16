using Domain.Entities.Vehicles;
using GenericRepository;

namespace Domain.Repositories.Vehicles;

public interface IVehicleImageRepository : IRepository<VehicleImage>
{
  // Belirtilen araca ait ana görseli (IsMain = true) veritabanından getirir.
  Task<VehicleImage?> GetMainImageAsync(Guid vehicleId, CancellationToken cancellationToken = default);

  // Belirtilen araca ait tüm görsellerin listesini getirir.
  Task<List<VehicleImage>> GetImagesByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);

  // Belirtilen araca ait silinmemiş (aktif) olan görsellerin listesini getirir.
  Task<List<VehicleImage>> GetActiveImagesByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
  Task<List<VehicleImage>> GetActiveImagesByVehicleWithTrackingAsync(Guid vehicleId, CancellationToken cancellationToken = default);

  // Belirtilen araca ait işaretlenmiş bir ana görselin (IsMain = true) bulunup bulunmadığını kontrol eder (True/False döner).
  Task<bool> HasMainImageAsync(Guid vehicleId, CancellationToken cancellationToken = default);

  // Belirtilen araca ait toplam görsel sayısını verir.
  Task<int> GetImageCountAsync(Guid vehicleId, CancellationToken cancellationToken = default);

  // Belirtilen araca ait tüm görsellerin ana görsel olma durumunu (IsMain) kapatarak false yapar (Genellikle yeni bir ana görsel seçilmeden önce eskileri sıfırlamak için kullanılır).
  Task SetAllAsNonMainAsync(Guid vehicleId, CancellationToken cancellationToken = default);

  Task AddVehicleImageAsync(Vehicle vehicle, string imageUrl, bool isMain, string? description, CancellationToken cancellationToken = default);

  void DeletePhysicalFile(string imageUrl);
  


  // public async Task AddVehicleImageAsync(Vehicle vehicle, string imageUrl, bool isMain, string? description, CancellationToken ct)
}