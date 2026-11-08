using Application.Behaviors;
using Application.Features.VehicleImages.Dto;
using Application.Services;
using Domain.Entities.Vehicles; // 🆕 VehicleStatus için
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleImages.Commands;

[Permission("VehicleImages.Create")]
public sealed record AddVehicleImageCommand(
    Guid VehicleId,
    List<FileUploadDto> Files,
    bool IsMain = false
) : IRequest<Result<AddVehicleImageResponse>>;

public sealed class AddVehicleImageCommandValidator : AbstractValidator<AddVehicleImageCommand>
{
  private const int MaxFileCount = 10;

  public AddVehicleImageCommandValidator()
  {
    RuleFor(x => x.VehicleId)
        .NotEmpty().WithMessage("Araç ID boş olamaz!");

    RuleFor(x => x.Files)
        .NotNull().WithMessage("En az bir dosya seçilmelidir!")
        .Must(f => f != null && f.Count > 0).WithMessage("En az bir dosya seçilmelidir!")
        .Must(f => f != null && f.Count <= MaxFileCount).WithMessage($"En fazla {MaxFileCount} resim yüklenebilir!");
  }
}

public sealed class AddVehicleImageCommandHandler(
    IVehicleRepository _vehicleRepo,
    IVehicleImageService _vehicleImageService,
    IUnitOfWork _unit,
    ILogger<AddVehicleImageCommandHandler> _logger
) : IRequestHandler<AddVehicleImageCommand, Result<AddVehicleImageResponse>>
{
  public async Task<Result<AddVehicleImageResponse>> Handle(AddVehicleImageCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("📸 Çoklu resim yükleme başladı. VehicleId: {VehicleId}, Dosya Sayısı: {Count}", _req.VehicleId, _req.Files?.Count ?? 0);

      // 1. Vehicle var mı kontrol et (Mevcut görselleriyle birlikte)
      var vehicle = await _vehicleRepo.GetVehicleWithImagesAsync(_req.VehicleId, _token);

      if (vehicle is null)
      {
        _logger.LogWarning("❌ Araç bulunamadı. VehicleId: {VehicleId}", _req.VehicleId);
        return Result<AddVehicleImageResponse>.Failure(404, "Araç bulunamadı!");
      }

      // 🆕 2. GÜVENLİK KONTROLÜ: Kiradaki veya bakımdaki araca resim eklenemez!
      if (vehicle.Status != VehicleStatus.Available)
      {
        _logger.LogWarning("⚠️ Araca resim eklenemez, operasyonel durumu uygun değil. VehicleId: {VehicleId}, Status: {Status}", _req.VehicleId, vehicle.Status);
        return Result<AddVehicleImageResponse>.Failure(400, $"Bu araç şu anda '{vehicle.Status}' durumunda olduğu için resim eklenemez!");
      }

      // 3. Dosya kontrolü (FluentValidation zaten yapıyor ama garantiye alalım)
      if (_req.Files is null || _req.Files.Count == 0)
        return Result<AddVehicleImageResponse>.Failure(400, "En az bir dosya seçilmelidir!");

      // 4. ✅ Servis ile resimleri kaydet
      var uploadedImages = await _vehicleImageService.SaveVehicleImagesAsync(
          vehicle,
          _req.Files,
          _req.IsMain,
          _token
      );

      // 5. Veritabanına kaydet
      await _unit.SaveChangesAsync(_token);

      _logger.LogInformation("✅ {Count} resim başarıyla yüklendi. VehicleId: {VehicleId}", uploadedImages.Count, _req.VehicleId);

      // 6. Response (Hatalı/atlanan dosya sayısını daha güvenli hesaplıyoruz)
      int failedCount = _req.Files.Count - uploadedImages.Count;

      return Result<AddVehicleImageResponse>.Succeed(new AddVehicleImageResponse(
          uploadedImages,
          uploadedImages.Count,
          failedCount
      ));
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Resim yükleme hatası. VehicleId: {VehicleId}", _req.VehicleId);
      return Result<AddVehicleImageResponse>.Failure(500, $"Resim yüklenirken bir hata oluştu: {_ex.Message}");
    }
  }
}