using Application.Features.VehicleImages.Dto;
using Application.Services;
using Domain.Entities.Vehicles;
using Domain.Repositories.Vehicles;
using GenericRepository;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class VehicleImageService(
                     IVehicleImageRepository _vehicleImageRepo,
                     IFileSlugifyService _fileSlug,
                     IUnitOfWork _unit,
                     ILogger<VehicleImageService> _logger
              ) : IVehicleImageService
{
  private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
  private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

  public async Task<List<UploadedImageInfo>> SaveVehicleImagesAsync(Vehicle _vehicle, List<FileUploadDto> _files, bool isMain = false, CancellationToken cancellationToken = default)
  {

    var uploadedImages = new List<UploadedImageInfo>();
    if (_files is null || _files.Count == 0) return uploadedImages;

    var brandSlug = _fileSlug.Slugify(_vehicle.Brand);
    var modelSlug = _fileSlug.Slugify(_vehicle.Model);
    var colorSlug = _fileSlug.Slugify(_vehicle.Color);

    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    var uploadPath = Path.Combine(webRootPath, "uploads", "vehicles", "images", $"{brandSlug.ToUpper()}_{modelSlug.ToUpper()}_{_vehicle.Id}");

    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

    foreach (var file in _files)
    {
      // Dosya kontrolü
      if (file is null || file.Length == 0) continue;

      // Dosya uzantısını kontrol et
      var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
      if (!_allowedExtensions.Contains(extension)) continue;

      // Dosya boyutunu kontrol et
      if (file.Length > MaxFileSize) continue;

      // Dosya adı oluştur
      var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
      var fileName = $"{brandSlug}_{modelSlug}_{_vehicle.Year}_{colorSlug}__{uniqueSuffix}";
      var filePath = Path.Combine(uploadPath, $"{fileName}{extension}");

      // Dosyayı kaydet
      using (var stream = new FileStream(filePath, FileMode.Create))
      {
        await file.FileStream.CopyToAsync(stream, cancellationToken);
      }

      // URL oluştur
      var imageUrl = $"/uploads/vehicles/images/{brandSlug.ToUpper()}_{modelSlug.ToUpper()}_{_vehicle.Id}/{fileName}{extension}";
      // Ana resim mi?
      var isMainForThis = (uploadedImages.Count == 0 && isMain);
      // Eğer ana resimse, diğerlerini non-main yap
      if (isMainForThis)
      {
        await _vehicleImageRepo.SetAllAsNonMainAsync(_vehicle.Id, cancellationToken);
      }
      // Database'e kaydet
      var newImage = _vehicle.AddImage(imageUrl, isMainForThis);
      _vehicleImageRepo.Add(newImage);

      uploadedImages.Add(new UploadedImageInfo(imageUrl, isMainForThis));
    }

    await _unit.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("✅ {Count} resim başarıyla kaydedildi. VehicleId: {VehicleId}", uploadedImages.Count, _vehicle.Id);

    return uploadedImages;
  }
}
