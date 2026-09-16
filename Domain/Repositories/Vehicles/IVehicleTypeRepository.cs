using Domain.Entities.Vehicles;
using GenericRepository;

namespace Domain.Repositories.Vehicles;

public interface IVehicleTypeRepository : IRepository<VehicleType>
{
  // Belirtilen ID'ye sahip araç tipini, ilişkili olduğu araçlarla (Vehicles) birlikte getirir.
  Task<VehicleType?> GetWithVehiclesAsync(Guid id, CancellationToken cancellationToken = default);

  // Sistemde aktif (silinmemiş) olan araç tiplerinin listesini getirir.
  Task<List<VehicleType>> GetActiveTypesAsync(CancellationToken cancellationToken = default);

  // Tüm araç tipleri ve bağlı modelleri getiriliyor
  // Task<List<VehicleType>> GetVehicleTypeWithModelsAsync(CancellationToken cancellationToken = default);

  // Araç tiplerini, her bir tipe ait araç sayılarıyla (Vehicle Count) birlikte liste olarak getirir.
  Task<List<VehicleTypeWithCountDto>> GetWithVehicleCountAsync(CancellationToken cancellationToken = default);

  // Verilen araç tipi adının sistemde benzersiz (unique) olup olmadığını kontrol eder. 
  // Güncellemelerde mevcut ID excludeId ile hariç tutulabilir.
  Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

public sealed record VehicleTypeWithCountDto(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    int DisplayOrder,
    int VehicleCount
);
