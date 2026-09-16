using Domain.Repositories;
using Domain.Repositories.Vehicles;
using GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleTypes.Commands;

public sealed record ToggleVehicleTypeStatusCommand(Guid Id) : IRequest<Result<bool>>;

public sealed class ToggleVehicleTypeStatusCommandHandler(
                        IVehicleTypeRepository _vehicleTypeRepo,
                        IUserRepository _userRepo,
                        IUnitOfWork _unit,
                        ILogger<ToggleVehicleTypeStatusCommandHandler> _logger
                    ) : IRequestHandler<ToggleVehicleTypeStatusCommand, Result<bool>>
{
  public async Task<Result<bool>> Handle(ToggleVehicleTypeStatusCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔄 ID: {Id} olan araç tipinin aktiflik durumu değiştiriliyor...", _req.Id);

      // 1. Kayıt var mı kontrol et (HasQueryFilter sayesinde silinmemişler gelir)
      var vehicleType = await _vehicleTypeRepo
                                .GetAll()
                                .Include(vt => vt.VehicleModels)
                                .FirstOrDefaultAsync(vt => vt.Id == _req.Id, _token);

      if (vehicleType is null)
      {
        _logger.LogWarning("⚠️ Durumu değiştirilecek araç tipi bulunamadı. ID: {Id}", _req.Id);
        return Result<bool>.Failure(404, "Araç tipi bulunamadı.");
      }

      // 2. Durumu değiştir ve metadata damgala
      var userId = _userRepo.GetCurrentUserId();

      // 2. Durumu değiştir (Active ↔ Passive)
      if (vehicleType.IsActive)
      { vehicleType.Deactivate(); }
      else
      { vehicleType.Activate(); }

      
      vehicleType.UpdateMetadata(userId);

      _vehicleTypeRepo.Update(vehicleType);
      await _unit.SaveChangesAsync(_token);

      _logger.LogInformation("✅ ID: {Id} olan araç tipinin yeni durumu: {IsActive}", _req.Id, vehicleType.IsActive);

      // Yeni aktiflik durumunu (true/false) yanıt olarak dönüyoruz
      return Result<bool>.Succeed(vehicleType.IsActive);


    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ ID: {Id} olan araç tipinin durumu değiştirilirken hata oluştu", _req.Id);
      return Result<bool>.Failure(500, $"Araç tipi durumu değiştirilirken hata oluştu: {_ex.Message}");
    }
  }
}
