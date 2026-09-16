using Application.Features.Vehicles.Dto;
using Domain.Entities.Vehicles;
using Domain.Repositories.Vehicles;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Vehicles;

public sealed class VehicleTypeRepository : Repository<VehicleType, AppDbContext>, IVehicleTypeRepository
{
  private readonly AppDbContext _context;

  public VehicleTypeRepository(AppDbContext context) : base(context)
  {
    this._context = context;
  }

  // Sistemde aktif (silinmemiş) olan araç tiplerinin listesini getirir.
  public async Task<List<VehicleType>> GetActiveTypesAsync(CancellationToken cancellationToken = default)
  {
    return await _context.VehicleTypes
                .Where(vt => !vt.IsDeleted && vt.IsActive)
                .Include(vt => vt.VehicleModels)
                    .ThenInclude(vm => vm.Vehicles)
                .OrderBy(vt => vt.DisplayOrder)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
  }

  // Araç tiplerini, her bir tipe ait araç sayılarıyla (Vehicle Count) birlikte liste olarak getirir.
  public async Task<List<VehicleTypeWithCountDto>> GetWithVehicleCountAsync(CancellationToken cancellationToken = default)
  {
    return await _context.VehicleTypes
        .Where(vt => !vt.IsDeleted && vt.IsActive)
        .Select(vt => new VehicleTypeWithCountDto(
            vt.Id,
            vt.Name,
            vt.Description,
            vt.Icon,
            vt.DisplayOrder,
            vt.VehicleModels.Count(v => !v.IsDeleted)
        ))
        .OrderBy(vt => vt.DisplayOrder)
        .AsNoTracking()
        .ToListAsync(cancellationToken);
  }

  // Belirtilen ID'ye sahip araç tipini, ilişkili olduğu araçlarla (Vehicles) birlikte getirir.
  public async Task<VehicleType?> GetWithVehiclesAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.VehicleTypes
        .Where(vt => !vt.IsDeleted && vt.Id == id)
        .Include(vt => vt.VehicleModels.Where(v => !v.IsDeleted))
        .AsNoTracking()
        .FirstOrDefaultAsync(cancellationToken);
  }

  // Verilen araç tipi adının sistemde benzersiz (unique) olup olmadığını kontrol eder. Güncellemelerde mevcut ID excludeId ile hariç tutulabilir.
  public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
  {
    var query = _context.VehicleTypes.Where(vt => !vt.IsDeleted && vt.Name.ToLower() == name.ToLower());

    if (excludeId.HasValue)
      query = query.Where(vt => vt.Id != excludeId.Value);

    return !await query.AnyAsync(cancellationToken);
  }

}
