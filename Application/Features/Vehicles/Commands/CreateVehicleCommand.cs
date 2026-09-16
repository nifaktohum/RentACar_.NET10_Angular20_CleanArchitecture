using Application.Features.Vehicles.Dto;
using Domain.Entities.Vehicles;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;
using Application.Features.VehicleImages.Dto;
using Application.Services;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Vehicles.Commands;

public sealed record CreateVehicleCommand(
                        string Brand,
                        string Model,
                        string Year,
                        string Plate,
                        string Color,
                        Guid VehicleModelId,
                        string FuelType,
                        string Transmission,
                        int SeatCount,
                        int DoorCount,
                        int? MinAge,
                        decimal DailyPrice,
                        string? Description,
                        IFormFile? ImageFile
                    ) : IRequest<Result<CreateVehicleDto>>;


public sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
  public CreateVehicleCommandValidator()
  {
    // Brand
    RuleFor(x => x.Brand)
        .NotEmpty().WithMessage("Marka boş olamaz!")
        .MaximumLength(50).WithMessage("Marka en fazla 50 karakter olabilir!");

    // Model
    RuleFor(x => x.Model)
        .NotEmpty().WithMessage("Model boş olamaz!")
        .MaximumLength(50).WithMessage("Model en fazla 50 karakter olabilir!");

    // Year
    RuleFor(x => x.Year)
        .NotEmpty().WithMessage("Yıl boş olamaz!")
        .Length(4).WithMessage("Yıl 4 haneli olmalı!")
        .Must(BeAValidYear).WithMessage("Yıl 1900 ile 2100 arasında olmalı!");

    // Plate
    RuleFor(x => x.Plate)
        .NotEmpty().WithMessage("Plaka boş olamaz!")
        .MaximumLength(20).WithMessage("Plaka en fazla 20 karakter olabilir!")
        .Matches(@"^[A-Z0-9\s-]+$").WithMessage("Plaka geçersiz format! (Sadece büyük harf, rakam, boşluk ve tire kullanabilirsiniz)");

    // Color
    RuleFor(x => x.Color)
        .NotEmpty().WithMessage("Renk boş olamaz!")
        .MaximumLength(30).WithMessage("Renk en fazla 30 karakter olabilir!");

    // VehicleTypeId
    RuleFor(x => x.VehicleModelId)
        .NotEmpty().WithMessage("Araç Modeli boş olamaz!");

    // FuelType
    RuleFor(x => x.FuelType)
        .NotEmpty().WithMessage("Yakıt tipi boş olamaz!")
        .MaximumLength(30).WithMessage("Yakıt tipi en fazla 30 karakter olabilir!");

    // Transmission
    RuleFor(x => x.Transmission)
        .NotEmpty().WithMessage("Vites tipi boş olamaz!")
        .MaximumLength(20).WithMessage("Vites tipi en fazla 20 karakter olabilir!");

    // SeatCount
    RuleFor(x => x.SeatCount)
        .GreaterThan(0).WithMessage("Koltuk sayısı 0'dan büyük olmalı!")
        .LessThanOrEqualTo(20).WithMessage("Koltuk sayısı en fazla 20 olabilir!");

    // DoorCount
    RuleFor(x => x.DoorCount)
        .GreaterThan(0).WithMessage("Kapı sayısı 0'dan büyük olmalı!")
        .LessThanOrEqualTo(10).WithMessage("Kapı sayısı en fazla 10 olabilir!");

    // MinAge
    RuleFor(x => x.MinAge)
        .GreaterThanOrEqualTo(18).When(x => x.MinAge.HasValue)
        .WithMessage("Minimum yaş 18'den küçük olamaz!")
        .LessThanOrEqualTo(99).When(x => x.MinAge.HasValue)
        .WithMessage("Minimum yaş 99'dan büyük olamaz!");

    // DailyPrice
    RuleFor(x => x.DailyPrice)
        .GreaterThan(0).WithMessage("Günlük fiyat 0'dan büyük olmalı!")
        .LessThanOrEqualTo(999999.99m).WithMessage("Günlük fiyat çok yüksek!");

    // Description
    RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir!");

    // ✅ ImageFile kontrolü (Opsiyonel)
    // Dosya varsa, boyut ve tip kontrolü yapılabilir
    // ✅ ImageFile kontrolü
    RuleFor(x => x.ImageFile)
        .Must(BeAValidImageFile).When(x => x.ImageFile is not null)
        .WithMessage("Geçersiz resim dosyası! (jpg, jpeg, png, webp, gif - max 5MB)");
  }

  private bool BeAValidYear(string year)
  {
    if (!int.TryParse(year, out int yearInt))
      return false;

    return yearInt >= 1900 && yearInt <= 2100;
  }

  private bool BeAValidImageFile(IFormFile? file)
  {
    if (file is null)
      return true;

    if (file.Length > 5 * 1024 * 1024)
      return false;

    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

    return allowedExtensions.Contains(extension);
  }
}


public sealed class CreateVehicleCommandHandler(
                          IVehicleRepository _vehicleRepo,
                          IVehicleModelRepository _vehicleModelRepo,
                          IVehicleImageService _imageService,
                          IUserRepository _userRepo,
                          IConfiguration _config,
                          IUnitOfWork _unit
                    ) : IRequestHandler<CreateVehicleCommand, Result<CreateVehicleDto>>
{
  public async Task<Result<CreateVehicleDto>> Handle(CreateVehicleCommand _req, CancellationToken _token)
  {
    // 1. Plate kontrolü
    var isPlateUnique = await _vehicleRepo.IsPlateUniqueAsync(_req.Plate, cancellationToken: _token);
    if (!isPlateUnique)
      return Result<CreateVehicleDto>.Failure($"'{_req.Plate}' plakalı araç zaten mevcut!");

    // 2. VehicleModel kontrolü
    var vehicleModel = await _vehicleModelRepo.GetByExpressionAsync(
        x => x.Id == _req.VehicleModelId && !x.IsDeleted,
        _token
    );
    if (vehicleModel is null)
      return Result<CreateVehicleDto>.Failure("Araç modeli bulunamadı!");

    // 3. User ID
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty)
      userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    // 4. Vehicle oluştur
    var vehicle = new Vehicle(
        _req.Brand,
        _req.Model,
        _req.Year,
        _req.Plate,
        _req.Color,
        _req.VehicleModelId,
        _req.FuelType,
        _req.Transmission,
        _req.SeatCount,
        _req.DoorCount,
        _req.MinAge,
        _req.DailyPrice,
        _req.Description,
        userId,
        true
    );

    // 5. ✅ ÖNCE VEHICLE'I KAYDET
    await _vehicleRepo.AddAsync(vehicle, _token);
    await _unit.SaveChangesAsync(_token);

    // 6. ✅ Resim varsa kaydet (IFormFile → FileUploadDto)
    if (_req.ImageFile is not null)
    {
      using var stream = _req.ImageFile.OpenReadStream();
      var fileUploadDto = new FileUploadDto(
          _req.ImageFile.FileName,      // FileName
          stream,                        // FileStream
          _req.ImageFile.ContentType,    // ContentType ✅
          _req.ImageFile.Length          // Length
      );

      await _imageService.SaveVehicleImagesAsync(
          vehicle,
          new List<FileUploadDto> { fileUploadDto },
          isMain: true,
          _token
      );
    }

    // 7. User name
    var userName = await _userRepo.GetUserNamesByIdsAsync(new List<Guid> { userId }, _token);
    string GetUserName(Guid id) => userName.GetValueOrDefault(id, "Bilinmiyor");

    // 8. Response
    // 8. Response (Images listesini de ekleyelim)
    var response = new CreateVehicleDto(
        vehicle.Id,
        vehicle.Brand,
        vehicle.Model,
        vehicle.Year,
        vehicle.Plate,
        vehicle.Color,
        vehicle.FuelType,
        vehicle.Transmission,
        vehicle.SeatCount,
        vehicle.DoorCount,
        vehicle.MinAge,
        vehicle.DailyPrice,
        vehicle.IsAvailable,
        vehicle.Description,
        vehicle.VehicleModelId,
        vehicle.VehicleModel?.Name ?? "Belirtilmemiş",
        vehicle.VehicleModel?.VehicleType?.Name ?? "Belirtilmemiş",
        // ✅ Resimleri buraya map etmelisin (Örnek DTO yapına göre uyarlayabilirsin)
        vehicle.Images.Select(i => new VehicleImageDto(
                 i.Id,
                 i.ImageUrl,
                 i.DisplayOrder,
                 i.IsMain,
                 i.Description,
                 i.IsActive,
                 i.CreatedAt,
                 i.CreatedBy,
                 GetUserName(i.CreatedBy),
                 i.UpdatedAt,
                 i.UpdatedBy,
                 i.UpdatedBy.HasValue ? GetUserName(i.UpdatedBy.Value) : null

          )).ToList(),

        vehicle.CreatedAt,
        vehicle.CreatedBy,
        GetUserName(vehicle.CreatedBy)
    );

    return Result<CreateVehicleDto>.Succeed(response);
  }
}