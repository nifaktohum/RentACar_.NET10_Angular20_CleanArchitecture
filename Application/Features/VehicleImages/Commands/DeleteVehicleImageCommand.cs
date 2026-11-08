using Application.Behaviors;
using Domain.Entities.Vehicles; // 🆕 VehicleStatus ve Image entity için
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleImages.Commands;

[Permission("VehicleImages.Delete")]
public sealed record DeleteVehicleImageCommand(Guid ImageId) : IRequest<Result<Unit>>;

public sealed class DeleteVehicleImageCommandHandler(
    IVehicleImageRepository _vehicleImageRepo,
    IVehicleRepository _vehicleRepo, // 🆕 Eklendi (Araç durumunu kontrol etmek için)
    IUserRepository _userRepo,
    IConfiguration _config,
    IUnitOfWork _unit,
    ILogger<DeleteVehicleImageCommandHandler> _logger
) : IRequestHandler<DeleteVehicleImageCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(DeleteVehicleImageCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🗑️ Resim silme işlemi başladı. ImageId: {ImageId}", _req.ImageId);

      // 1. Resmi bul
      var image = await _vehicleImageRepo.GetByExpressionWithTrackingAsync(x => x.Id == _req.ImageId && !x.IsDeleted, _token);
      if (image is null)
      {
        _logger.LogWarning("❌ Resim bulunamadı. ImageId: {ImageId}", _req.ImageId);
        return Result<Unit>.Failure(404, "Resim bulunamadı!");
      }

      // 🆕 2. GÜVENLİK KONTROLÜ: Araç kiradaysa veya bakımdaysa resim silinemez!
      var vehicle = await _vehicleRepo.FirstOrDefaultAsync(v => v.Id == image.VehicleId, _token);
      if (vehicle is null)
        return Result<Unit>.Failure(404, "Araç bulunamadı!");

      if (vehicle.Status != VehicleStatus.Available)
      {
        _logger.LogWarning("⚠️ Resim silinemez, araç operasyonel durumda. VehicleId: {VehicleId}, Status: {Status}", vehicle.Id, vehicle.Status);
        return Result<Unit>.Failure(400, $"Bu araç şu anda '{vehicle.Status}' durumunda olduğu için resim silinemez!");
      }

      var vehicleId = image.VehicleId;
      var wasMain = image.IsMain;

      // 3. Soft Delete işlemi
      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty)
        userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

      if (image.IsMain)
      {
        image.SetAsNonMain();
      }

      image.SoftDelete(userId);

      // ⭐ 4. Eğer ana resim silindiyse yeni ana resim belirle
      if (wasMain)
      {
        var remainingImages = await _vehicleImageRepo.GetActiveImagesByVehicleWithTrackingAsync(vehicleId, _token);

        var nextImage = remainingImages
            .Where(x => x.Id != image.Id && !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .FirstOrDefault();

        if (nextImage is not null)
        {
          // Tüm resimlerin IsMain'ini false yap
          foreach (var img in remainingImages.Where(x => x.Id != image.Id))
          {
            if (img.IsMain) img.SetAsNonMain();
          }

          // Yeni resmi ana resim yap
          nextImage.SetAsMain();
          _logger.LogInformation("🔄 Yeni ana resim belirlendi. ImageId: {ImageId}", nextImage.Id);
        }
        else
        {
          _logger.LogWarning("⚠️ Araç için hiç aktif resim kalmadı. VehicleId: {VehicleId}", vehicleId);
        }
      }

      // 5. Veritabanı değişikliklerini kaydet
      await _unit.SaveChangesAsync(_token);

      // 🆕 6. Veritabanı işlemi BAŞARILI olduktan sonra fiziksel dosyayı sil
      // (Böylece DB kaydı başarısız olursa dosya kaybolmaz)
      try
      {
        _vehicleImageRepo.DeletePhysicalFile(image.ImageUrl);
        _logger.LogInformation("🗑️ Fiziksel dosya diskten silindi. Url: {Url}", image.ImageUrl);
      }
      catch (Exception fileEx)
      {
        // Dosya silinemese bile DB'den silindiği için işlemi patlatmıyoruz.
        _logger.LogError(fileEx, "⚠️ Fiziksel dosya silinirken hata oluştu (DB'den silindi). Url: {Url}", image.ImageUrl);
      }

      _logger.LogInformation("✅ Resim başarıyla silindi. ImageId: {ImageId}, VehicleId: {VehicleId}", image.Id, vehicleId);

      return Result<Unit>.Succeed(Unit.Value);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Resim silme işlemi sırasında hata oluştu. ImageId: {ImageId}", _req.ImageId);
      return Result<Unit>.Failure(500, "Resim silinirken bir hata oluştu!");
    }
  }
}