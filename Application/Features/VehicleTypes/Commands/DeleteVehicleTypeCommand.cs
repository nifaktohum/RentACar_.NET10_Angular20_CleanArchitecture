using Domain.Repositories;
using Domain.Repositories.Vehicles;
using GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleTypes.Commands;

public sealed record DeleteVehicleTypeCommand(Guid Id) : IRequest<Result<Unit>>;

public sealed class DeleteVehicleTypeCommandHandler(
                        IVehicleTypeRepository _vehicleTypeRepo,
                        IUserRepository _userRepo,
                        IUnitOfWork _unit,
                        IConfiguration _config,
                        ILogger<DeleteVehicleTypeCommandHandler> _logger
                    ) : IRequestHandler<DeleteVehicleTypeCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(DeleteVehicleTypeCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🗑️ ID: {Id} olan araç tipi siliniyor...", _req.Id);

      // 1. Araç tipini ilişkili modelleriyle getir
      // Configuration'daki HasQueryFilter(!IsDeleted) sayesinde silinmişler otomatik elenir.
      var vehicleType = await _vehicleTypeRepo
          .GetAll()
          .Include(vt => vt.VehicleModels)
          .FirstOrDefaultAsync(vt => vt.Id == _req.Id, _token);

      if (vehicleType is null)
      {
        _logger.LogWarning("⚠️ Silinecek araç tipi bulunamadı. ID: {Id}", _req.Id);
        return Result<Unit>.Failure(404, "Silinmek istenen araç tipi bulunamadı.");
      }

      // 2. İş Kuralı: Bu tipe bağlı aktif araç modeli var mı?
      var hasActiveModels = vehicleType.VehicleModels.Any(vm => !vm.IsDeleted);
      if (hasActiveModels)
      {
        _logger.LogWarning("⚠️ Araç tipi silinemedi. Bağlı aktif modeller var. ID: {Id}", _req.Id);
        return Result<Unit>.Failure(400, $"'{vehicleType.Name}' tipine bağlı araç modelleri olduğu için bu kayıt silinemez. Önce ilişkili modelleri silmelisiniz.");
      }

      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty)
        userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

      // 3. Soft Delete İşlemi
      vehicleType.SoftDelete(userId);
      _vehicleTypeRepo.Update(vehicleType);

      await _unit.SaveChangesAsync(_token);

      _logger.LogInformation("✅ ID: {Id} olan araç tipi başarıyla silindi.", _req.Id);
      return Result<Unit>.Succeed(Unit.Value);

    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ ID: {Id} olan araç tipi silinirken hata oluştu", _req.Id);
      return Result<Unit>.Failure(500, $"Araç tipi silinirken beklenmeyen bir hata oluştu: {_ex.Message}");
    }
  }
}