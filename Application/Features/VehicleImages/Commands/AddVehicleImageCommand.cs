using System.Globalization;
using Application.Features.VehicleImages.Dto;
using Application.Features.Vehicles.Dto;
using Application.Services;
using Domain.Entities.Vehicles;
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleImages.Commands;

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
  private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
  private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
  private const int MaxFileCount = 10;

  public async Task<Result<AddVehicleImageResponse>> Handle(AddVehicleImageCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("📸 Çoklu resim yükleme başladı. VehicleId: {VehicleId}, Dosya Sayısı: {Count}", _req.VehicleId, _req.Files?.Count ?? 0);
      // 1. Vehicle var mı kontrol et
      var vehicle = await _vehicleRepo.GetVehicleWithImagesAsync(_req.VehicleId, _token);

      if (vehicle is null)
      {
        _logger.LogWarning("❌ Araç bulunamadı. VehicleId: {VehicleId}", _req.VehicleId);
        return Result<AddVehicleImageResponse>.Failure(404, "Araç bulunamadı!");
      }

      // 2. Dosya kontrolü
      if (_req.Files is null || _req.Files.Count == 0)
        return Result<AddVehicleImageResponse>.Failure(400, "En az bir dosya seçilmelidir!");

      // 3. ✅ Servis ile resimleri kaydet
      var uploadedImages = await _vehicleImageService.SaveVehicleImagesAsync(
          vehicle,
          _req.Files,
          _req.IsMain,
          _token
      );

      #region SaveVehicleImagesAsync
      /*
    if (_req.Files.Count > MaxFileCount)
      return Result<AddVehicleImageResponse>.Failure(400, $"En fazla {MaxFileCount} resim yüklenebilir!");

    var brandSlug = _fileSlug.Slugify(vehicle.Brand);
    var modelSlug = _fileSlug.Slugify(vehicle.Model);
    var colorSlug = _fileSlug.Slugify(vehicle.Color);

    var uploadedImages = new List<UploadedImageInfo>();
    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    var uploadPath = Path.Combine(webRootPath, "uploads", "vehicles", "images", $"{brandSlug.ToUpper()}_{modelSlug.ToUpper()}_{vehicle.Id}");

    if (!Directory.Exists(uploadPath))
      Directory.CreateDirectory(uploadPath);

    for (int i = 0; i < _req.Files.Count; i++)
    {
      var file = _req.Files[i];


      // 3.1 Dosya kontrolü
      if (file is null || file.Length == 0)
        continue;


      // 3.2 Dosya uzantısını kontrol et
      var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
      if (!_allowedExtensions.Contains(extension))
        continue;

      // 3.3 Dosya boyutunu kontrol et
      if (file.Length > MaxFileSize)
        continue;

      // 3.4 Dosya adı oluştur
      var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
      var fileName = $"{brandSlug}_{modelSlug}_{vehicle.Year}_{colorSlug}__{uniqueSuffix}";
      //         var filePath = Path.Combine(uploadPath, $"{fileName}{extension}");
      var filePath = Path.Combine(uploadPath, $"{fileName}{extension}");

      // 3.5 Dosyayı kaydet
      using (var stream = new FileStream(filePath, FileMode.Create))
      {
        await file.FileStream.CopyToAsync(stream, _token);
      }

      // 3.6 URL oluştur
      var imageUrl = $"/uploads/vehicles/images/{brandSlug.ToUpper()}_{modelSlug.ToUpper()}_{vehicle.Id}/{fileName}{extension}";

      // 3.7 İlk resim main olabilir (eğer IsMain true ise)
      var isMainForThis = (uploadedImages.Count == 0 && _req.IsMain);

      // 3.8 Eğer ana resimse, diğerlerini non-main yap
      if (isMainForThis)
      {
        await _vehicleImageRepo.SetAllAsNonMainAsync(_req.VehicleId, _token);
      }

      // 3.9 Database'e kaydet
      var newImage = vehicle.AddImage(imageUrl, isMainForThis);
      _vehicleImageRepo.Add(newImage);

      uploadedImages.Add(new UploadedImageInfo(imageUrl, isMainForThis));

    }
      */
      #endregion

      // 9. Veritabanına kaydet
      await _unit.SaveChangesAsync(_token);


      _logger.LogInformation("✅ {Count} resim başarıyla yüklendi. VehicleId: {VehicleId}", uploadedImages.Count, _req.VehicleId);

      return Result<AddVehicleImageResponse>.Succeed(new AddVehicleImageResponse(
              uploadedImages,
              uploadedImages.Count,
              _req.Files.Count - uploadedImages.Count
          ));

    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Resim yükleme hatası. VehicleId: {VehicleId}", _req.VehicleId);
      return Result<AddVehicleImageResponse>.Failure(500, $"Resim yüklenirken bir hata oluştu: {_ex.Message}");
    }
  }
}