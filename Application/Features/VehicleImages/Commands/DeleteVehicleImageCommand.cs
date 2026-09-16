using Domain.Repositories;
using Domain.Repositories.Vehicles;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleImages.Commands;

public sealed record DeleteVehicleImageCommand(Guid ImageId) : IRequest<Result<Unit>>;

public sealed class DeleteVehicleImageCommandHandler(
                        IVehicleImageRepository _vehicleImageRepo,
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
      var image = await _vehicleImageRepo.GetByExpressionWithTrackingAsync(x => x.Id == _req.ImageId, _token);
      if (image is null)
        return Result<Unit>.Failure(404, "Resim bulunamadı!");

      var vehicleId = image.VehicleId;
      var wasMain = image.IsMain;

      // Fiziksel dosyayı sil
      _vehicleImageRepo.DeletePhysicalFile(image.ImageUrl);

      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty)
        userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

      // 1. Silinen resmin IsMain'ini false yap
      if (image.IsMain)
      {
        image.SetAsNonMain();
        _logger.LogInformation("📌 Silinen resmin IsMain özelliği false yapıldı. ImageId: {ImageId}", image.Id);
      }

      // 2. Soft Delete işlemi
      image.SoftDelete(userId);

      // ⭐ 3. Eğer ana resim silindiyse yeni ana resim belirle
      if (wasMain)
      {
        // Aktif resimleri tracking ile getir
        var remainingImages = await _vehicleImageRepo.GetActiveImagesByVehicleWithTrackingAsync(vehicleId, _token);

        // Silinen resmi hariç tut ve sıralı olarak ilkini al
        var nextImage = remainingImages
            .Where(x => x.Id != image.Id && !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .FirstOrDefault();

        if (nextImage is not null)
        {
          // ⭐ 4. Güvenlik: Tüm resimlerin IsMain'ini false yap (veri tutarlılığı için)
          foreach (var img in remainingImages.Where(x => x.Id != image.Id))
          {
            if (img.IsMain)
            {
              img.SetAsNonMain();
              _logger.LogDebug("📌 Eski ana resim işareti kaldırıldı. ImageId: {ImageId}", img.Id);
            }
          }

          // ⭐ 5. Yeni resmi ana resim yap
          nextImage.SetAsMain();
          _logger.LogInformation("🔄 Yeni ana resim belirlendi. ImageId: {ImageId}, DisplayOrder: {DisplayOrder}",
              nextImage.Id, nextImage.DisplayOrder);
        }
        else
        {
          _logger.LogWarning("⚠️ Araç için hiç aktif resim kalmadı. VehicleId: {VehicleId}", vehicleId);
        }
      }

      // Tüm değişiklikleri kaydet
      await _unit.SaveChangesAsync(_token);

      _logger.LogInformation("✅ Resim başarıyla silindi. ImageId: {ImageId}, VehicleId: {VehicleId}",
          image.Id, vehicleId);

      return Result<Unit>.Succeed(Unit.Value);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Resim silme işlemi sırasında hata oluştu. ImageId: {ImageId}", _req.ImageId);
      return Result<Unit>.Failure(500, "Resim silinirken bir hata oluştu!");
    }
  }
}