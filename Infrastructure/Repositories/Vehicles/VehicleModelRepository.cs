using Domain.Entities.Vehicles;
using Domain.Repositories.Vehicles;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Vehicles;

public sealed class VehicleModelRepository : Repository<VehicleModel, AppDbContext>, IVehicleModelRepository
{
  private readonly AppDbContext _context;

  public VehicleModelRepository(AppDbContext context) : base(context)
  {
    _context = context;
  }
  // Aynı marka ve model zaten var mı kontrol et
  public async Task<bool> ExistsByBrandAndNameAsync(string brand, string name, CancellationToken cancellationToken = default)
  {
    return await _context.VehicleModels.AnyAsync(
                              vm => vm.Brand.ToLower() == brand.ToLower() &&
                                    vm.Name.ToLower() == name.ToLower() &&
                                    !vm.IsDeleted, cancellationToken);
  }

  public async Task<bool> ExistsByBrandAndNameExcludeSelfAsync(string brand, string name, Guid excludeId, CancellationToken cancellationToken = default)
  {
    return await _context.VehicleModels.AnyAsync(
                              x => x.Brand == brand &&
                                    x.Name == name &&
                                    x.Id != excludeId &&
                                    !x.IsDeleted,
                                    cancellationToken);
  }

  // ✅ Fiziksel silme
  public async Task HardDeleteAsync(Guid id, CancellationToken cancellationToken = default)
  {
    // IgnoreQueryFilters eklenmezse IsDeleted=true olan kayıtlar ExecuteDelete ile bulunamaz
    await _context.VehicleModels
        .IgnoreQueryFilters()
        .Where(x => x.Id == id)
        .ExecuteDeleteAsync(cancellationToken);
  }
}
