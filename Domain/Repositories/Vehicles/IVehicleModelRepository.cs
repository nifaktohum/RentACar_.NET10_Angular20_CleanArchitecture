using Domain.Entities.Vehicles;
using GenericRepository;

namespace Domain.Repositories.Vehicles;

public interface IVehicleModelRepository: IRepository<VehicleModel>
{
  // Aynı marka ve model zaten var mı kontrol et
  Task<bool> ExistsByBrandAndNameAsync(string brand, string name, CancellationToken cancellationToken = default);
  // Kendi hariç kontrol
  Task<bool> ExistsByBrandAndNameExcludeSelfAsync(string brand, string name, Guid excludeId, CancellationToken cancellationToken = default );

  // ✅ Fiziksel silme için
  Task HardDeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
