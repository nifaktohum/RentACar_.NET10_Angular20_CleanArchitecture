using Application.Behaviors;
using Domain.Entities.Vehicles; // 🆕 VehicleStatus için eklendi
using Domain.Repositories.Vehicles;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleImages.Commands;

[Permission("VehicleImages.Update")]
public sealed record SetMainVehicleImageCommand(Guid VehicleId, Guid ImageId) : IRequest<Result<Unit>>;

public sealed class SetMainVehicleImageCommandHandler(
    IVehicleImageRepository _vehicleImageRepo,
    IVehicleRepository _vehicleRepo, // 🆕 Eklendi (Durum kontrolü için)
    IUnitOfWork _unit,
    ILogger<SetMainVehicleImageCommandHandler> _logger
) : IRequestHandler<SetMainVehicleImageCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(SetMainVehicleImageCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔄 Ana resim değiştirme başladı. VehicleId: {VehicleId}, ImageId: {ImageId}", _req.VehicleId, _req.ImageId);

      // 1. Resmi bul
      var image = await _vehicleImageRepo.GetByExpressionWithTrackingAsync(
          x => x.Id == _req.ImageId && !x.IsDeleted && x.IsActive,
          _token);

      if (image is null)
      {
        _logger.LogWarning("❌ Resim bulunamadı. ImageId: {ImageId}", _req.ImageId);
        return Result<Unit>.Failure(404, "Resim bulunamadı veya aktif değil!");
      }

      // 🆕 2. GÜVENLİK KONTROLÜ: İstekteki VehicleId ile resmin ait olduğu araç eşleşiyor mu?
      if (image.VehicleId != _req.VehicleId)
      {
        _logger.LogWarning("⚠️ Güvenlik ihlali: Resim farklı bir araca ait. İstenen VehicleId: {ReqVehicleId}, Gerçek VehicleId: {RealVehicleId}", _req.VehicleId, image.VehicleId);
        return Result<Unit>.Failure(400, "Bu resim belirtilen araca ait değil!");
      }

      // 🆕 3. GÜVENLİK KONTROLÜ: Araç kiradaysa veya bakımdaysa ana resim değiştirilemez!
      var vehicle = await _vehicleRepo.FirstOrDefaultAsync(v => v.Id == _req.VehicleId, _token);
      if (vehicle is null)
        return Result<Unit>.Failure(404, "Araç bulunamadı!");

      if (vehicle.Status != VehicleStatus.Available)
      {
        _logger.LogWarning("⚠️ Ana resim değiştirilemez, araç operasyonel durumda. VehicleId: {VehicleId}, Status: {Status}", vehicle.Id, vehicle.Status);
        return Result<Unit>.Failure(400, $"Bu araç şu anda '{vehicle.Status}' durumunda olduğu için ana resim değiştirilemez!");
      }

      // 4. Eğer zaten ana resimse, işlem yapma
      if (image.IsMain)
      {
        _logger.LogInformation("ℹ️ Seçilen resim zaten ana resim. İşlem atlandı. ImageId: {ImageId}", image.Id);
        return Result<Unit>.Succeed(Unit.Value);
      }

      // 5. Aynı araca ait tüm resimlerin IsMain'ini false yap
      var vehicleImages = await _vehicleImageRepo.GetActiveImagesByVehicleWithTrackingAsync(image.VehicleId, _token);

      foreach (var img in vehicleImages)
      {
        if (img.IsMain)
          img.SetAsNonMain();
      }

      // 6. Seçilen resmi ana resim yap
      image.SetAsMain();

      // 7. Değişiklikleri kaydet
      await _unit.SaveChangesAsync(_token);

      _logger.LogInformation("✅ Ana resim başarıyla güncellendi. ImageId: {ImageId}, VehicleId: {VehicleId}", image.Id, image.VehicleId);

      return Result<Unit>.Succeed(Unit.Value);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Ana resim güncellenirken hata oluştu. ImageId: {ImageId}", _req.ImageId);
      return Result<Unit>.Failure(500, "Ana resim güncellenirken bir hata oluştu!");
    }
  }
}