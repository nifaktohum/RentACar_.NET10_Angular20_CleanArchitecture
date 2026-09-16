using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Repositories.Vehicles;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleImages.Commands;

public sealed record SetMainVehicleImageCommand(Guid VehicleId, Guid ImageId) : IRequest<Result<Unit>>;

public sealed class SetMainVehicleImageCommandHandler(
                          IVehicleImageRepository _vehicleImageRepo,
                          IUnitOfWork _unit,
                          ILogger<SetMainVehicleImageCommandHandler> _logger
                    ) : IRequestHandler<SetMainVehicleImageCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(SetMainVehicleImageCommand _req, CancellationToken _token)
  {
    try
    {
      // 1. Resmi bul
      var image = await _vehicleImageRepo.GetByExpressionWithTrackingAsync(
          x => x.Id == _req.ImageId && !x.IsDeleted && x.IsActive,
          _token);

      if (image is null)
        return Result<Unit>.Failure(404, "Resim bulunamadı veya aktif değil!");

      // 2. Eğer zaten ana resimse, işlem yapma
      if (image.IsMain) return Result<Unit>.Succeed(Unit.Value);

      // 3. Aynı araca ait tüm resimlerin IsMain'ini false yap
      var vehicleImages = await _vehicleImageRepo.GetActiveImagesByVehicleWithTrackingAsync(image.VehicleId, _token);

      foreach (var img in vehicleImages)
      {
        if (img.IsMain)
          img.SetAsNonMain();
      }

      // 4. Seçilen resmi ana resim yap
      image.SetAsMain();

      // 5. Değişiklikleri kaydet
      await _unit.SaveChangesAsync(_token);
      _logger.LogInformation($"✅ Ana resim başarıyla güncellendi. ImageId: {image.Id}, VehicleId: {image.VehicleId}");

      return Result<Unit>.Succeed(Unit.Value);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, $"❌ Ana resim güncellenirken hata oluştu. ImageId: {_req.ImageId}");
      return Result<Unit>.Failure(500, "Ana resim güncellenirken bir hata oluştu!");
    }
  }
}

