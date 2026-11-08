using Application.Features.Vehicles.Dto;
using Application.Features.VehicleImages.Dto;
using Application.Services;
using Domain.Entities.Vehicles;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using TS.Result;
using Application.Behaviors;

namespace Application.Features.Vehicles.Commands;

[Permission("Vehicles.Create")]
public sealed record CreateVehicleCommand(
    // --- Temel Bilgiler ---
    string Brand,
    string Model,
    string Year,
    string Plate,
    string Color,
    Guid VehicleModelId,
    Guid BranchId,                         
    string VinNumber,                     
    string EngineNumber,                   

    // --- Teknik Özellikler ---
    string FuelType,
    string Transmission,
    string TractionType,                   
    decimal EngineVolume,                  
    int EnginePower,                       
    decimal FuelConsumption,              
    int SeatCount,
    int DoorCount,

    // --- Ticari & Yasal Bilgiler ---
    int? MinAge,
    decimal DailyPrice,
    decimal DepositAmount,                 
    decimal WeeklyDiscountRate,            
    decimal MonthlyDiscountRate,           

    // --- Bakım & Durum ---
    int CurrentKilometer,                  
    int LastMaintenanceKm,                 
    int NextMaintenanceKm,                 
    DateTimeOffset LastMaintenanceDate,    
    DateTimeOffset InspectionDate,         
    DateTimeOffset InsuranceEndDate,       
    DateTimeOffset CascoEndDate,           
    string TireStatus,                     
    string GeneralStatus,                  

    string? Description,
    IFormFile? ImageFile
) : IRequest<Result<CreateVehicleDto>>;


public sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
  public CreateVehicleCommandValidator()
  {
    // --- Temel Bilgiler ---
    RuleFor(x => x.Brand).NotEmpty().MaximumLength(50);
    RuleFor(x => x.Model).NotEmpty().MaximumLength(50);
    RuleFor(x => x.Year).NotEmpty().Length(4).Must(BeAValidYear).WithMessage("Yıl 1900-2100 arası olmalı!");
    RuleFor(x => x.Plate).NotEmpty().MaximumLength(20).Matches(@"^[A-Z0-9\s-]+$");
    RuleFor(x => x.Color).NotEmpty().MaximumLength(30);
    RuleFor(x => x.VehicleModelId).NotEmpty();
    RuleFor(x => x.BranchId).NotEmpty().WithMessage("Şube seçilmelidir!"); // 🆕
    RuleFor(x => x.VinNumber).NotEmpty().Length(17).WithMessage("Şasi numarası 17 karakter olmalıdır!"); // 🆕
    RuleFor(x => x.EngineNumber).NotEmpty().MaximumLength(30); // 🆕

    // --- Teknik Özellikler ---
    RuleFor(x => x.FuelType).NotEmpty().MaximumLength(20);
    RuleFor(x => x.Transmission).NotEmpty().MaximumLength(20);
    RuleFor(x => x.TractionType).NotEmpty().MaximumLength(20);
    RuleFor(x => x.EngineVolume).GreaterThan(0);
    RuleFor(x => x.EnginePower).GreaterThan(0);
    RuleFor(x => x.FuelConsumption).GreaterThan(0);
    RuleFor(x => x.SeatCount).InclusiveBetween(1, 60);
    RuleFor(x => x.DoorCount).InclusiveBetween(1, 10);

    // --- Ticari & Yasal ---
    RuleFor(x => x.MinAge).InclusiveBetween(18, 80).When(x => x.MinAge.HasValue);
    RuleFor(x => x.DailyPrice).GreaterThan(0).LessThanOrEqualTo(999999.99m);
    RuleFor(x => x.DepositAmount).GreaterThanOrEqualTo(0);
    RuleFor(x => x.WeeklyDiscountRate).InclusiveBetween(0, 100);
    RuleFor(x => x.MonthlyDiscountRate).InclusiveBetween(0, 100);

    // --- Bakım & Durum ---
    RuleFor(x => x.CurrentKilometer).GreaterThanOrEqualTo(0);
    RuleFor(x => x.LastMaintenanceKm).GreaterThanOrEqualTo(0).LessThanOrEqualTo(x => x.CurrentKilometer).WithMessage("Son bakım km'si güncel km'den büyük olamaz!");
    RuleFor(x => x.NextMaintenanceKm).GreaterThan(x => x.LastMaintenanceKm).WithMessage("Sonraki bakım km'si son bakımdan büyük olmalı!");
    RuleFor(x => x.TireStatus).NotEmpty();
    RuleFor(x => x.GeneralStatus).NotEmpty();

    RuleFor(x => x.Description).MaximumLength(1000);

    // Resim kontrolü
    RuleFor(x => x.ImageFile)
        .Must(BeAValidImageFile).When(x => x.ImageFile is not null)
        .WithMessage("Geçersiz resim dosyası! (max 5MB, jpg/png/webp)");
  }

  private bool BeAValidYear(string year) =>
      int.TryParse(year, out int y) && y >= 1900 && y <= 2100;

  private bool BeAValidImageFile(IFormFile? file)
  {
    if (file is null) return true;
    if (file.Length > 5 * 1024 * 1024) return false;
    var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    return allowed.Contains(Path.GetExtension(file.FileName).ToLowerInvariant());
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
    // 1. Benzersizlik Kontrolleri (Plaka & VIN)
    var isPlateUnique = await _vehicleRepo.IsPlateUniqueAsync(_req.Plate, cancellationToken: _token);
    if (!isPlateUnique)
      return Result<CreateVehicleDto>.Failure($"'{_req.Plate}' plakalı araç zaten mevcut!");

    // 🆕 VIN kontrolü (Repository'de bu metodun olduğundan emin ol)
    // var isVinUnique = await _vehicleRepo.IsVinUniqueAsync(_req.VinNumber, cancellationToken: _token);
    // if (!isVinUnique) return Result<CreateVehicleDto>.Failure($"'{_req.VinNumber}' şasi numaralı araç zaten mevcut!");

    // 2. VehicleModel kontrolü
    var vehicleModel = await _vehicleModelRepo.GetByExpressionAsync(
        x => x.Id == _req.VehicleModelId && !x.IsDeleted, _token);
    if (vehicleModel is null)
      return Result<CreateVehicleDto>.Failure("Araç modeli bulunamadı!");

    // 3. User ID
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty)
      userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    // 4. Vehicle oluştur (TÜM YENİ PARAMETRELER EKLENDİ)
    var vehicle = new Vehicle(
        brand: _req.Brand,
        model: _req.Model,
        year: _req.Year,
        plate: _req.Plate,
        color: _req.Color,
        vehicleModelId: _req.VehicleModelId,
        branchId: _req.BranchId,
        vinNumber: _req.VinNumber,
        engineNumber: _req.EngineNumber,
        fuelType: _req.FuelType,
        transmission: _req.Transmission,
        tractionType: _req.TractionType,
        engineVolume: _req.EngineVolume,
        enginePower: _req.EnginePower,
        fuelConsumption: _req.FuelConsumption,
        seatCount: _req.SeatCount,
        doorCount: _req.DoorCount,
        minAge: _req.MinAge,
        dailyPrice: _req.DailyPrice,
        depositAmount: _req.DepositAmount,
        weeklyDiscountRate: _req.WeeklyDiscountRate,
        monthlyDiscountRate: _req.MonthlyDiscountRate,
        currentKilometer: _req.CurrentKilometer,
        lastMaintenanceKm: _req.LastMaintenanceKm,
        nextMaintenanceKm: _req.NextMaintenanceKm,
        lastMaintenanceDate: _req.LastMaintenanceDate,
        inspectionDate: _req.InspectionDate,
        insuranceEndDate: _req.InsuranceEndDate,
        cascoEndDate: _req.CascoEndDate,
        tireStatus: _req.TireStatus,
        generalStatus: _req.GeneralStatus,
        description: _req.Description,
        createdBy: userId,
        isActive: true
    );

    // 5. ÖNCE VEHICLE'I KAYDET
    await _vehicleRepo.AddAsync(vehicle, _token);
    await _unit.SaveChangesAsync(_token);

    // 6. Resim varsa kaydet
    if (_req.ImageFile is not null)
    {
      using var stream = _req.ImageFile.OpenReadStream();
      var fileUploadDto = new FileUploadDto(
          _req.ImageFile.FileName,
          stream,
          _req.ImageFile.ContentType,
          _req.ImageFile.Length
      );

      await _imageService.SaveVehicleImagesAsync(
          vehicle,
          new List<FileUploadDto> { fileUploadDto },
          isMain: true,
          _token
      );

      // Resim kaydedildikten sonra unit of work'ü tekrar kaydetmek gerekebilir
      await _unit.SaveChangesAsync(_token);
    }

    // 7. User name
    var userName = await _userRepo.GetUserNamesByIdsAsync(new List<Guid> { userId }, _token);
    string GetUserName(Guid id) => userName.GetValueOrDefault(id, "Bilinmiyor");

    // 8. Response
    var response = new CreateVehicleDto(
        Id: vehicle.Id,

        // Temel
        Brand: vehicle.Brand,
        Model: vehicle.Model,
        Year: vehicle.Year,
        Plate: vehicle.Plate,
        Color: vehicle.Color,
        VehicleModelId: vehicle.VehicleModelId,
        VehicleModelName: vehicle.VehicleModel?.Name ?? "Belirtilmemiş",
        VehicleTypeName: vehicle.VehicleModel?.VehicleType?.Name ?? "Belirtilmemiş",
        BranchId: vehicle.BranchId,

        // Teknik
        VinNumber: vehicle.VinNumber,
        EngineNumber: vehicle.EngineNumber,
        FuelType: vehicle.FuelType,
        Transmission: vehicle.Transmission,
        TractionType: vehicle.TractionType,
        EngineVolume: vehicle.EngineVolume,
        EnginePower: vehicle.EnginePower,
        FuelConsumption: vehicle.FuelConsumption,
        SeatCount: vehicle.SeatCount,
        DoorCount: vehicle.DoorCount,

        // Ticari
        MinAge: vehicle.MinAge,
        DailyPrice: vehicle.DailyPrice,
        DepositAmount: vehicle.DepositAmount,
        WeeklyDiscountRate: vehicle.WeeklyDiscountRate,
        MonthlyDiscountRate: vehicle.MonthlyDiscountRate,

        // Bakım & Durum
        CurrentKilometer: vehicle.CurrentKilometer,
        LastMaintenanceKm: vehicle.LastMaintenanceKm,
        NextMaintenanceKm: vehicle.NextMaintenanceKm,
        LastMaintenanceDate: vehicle.LastMaintenanceDate,
        InspectionDate: vehicle.InspectionDate,
        InsuranceEndDate: vehicle.InsuranceEndDate,
        CascoEndDate: vehicle.CascoEndDate,
        TireStatus: vehicle.TireStatus,
        GeneralStatus: vehicle.GeneralStatus,

        // Sistem
        IsAvailable: vehicle.IsAvailable,
        Status: vehicle.Status.ToString(), // Enum'ı string'e çevirdik
        Description: vehicle.Description,

        // Resimler
        Images: vehicle.Images.Select(i => new VehicleImageDto(
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

        // Audit
        CreatedAt: vehicle.CreatedAt,
        CreatedBy: vehicle.CreatedBy,
        CreatedByName: GetUserName(vehicle.CreatedBy)
    );
    return Result<CreateVehicleDto>.Succeed(response);
  }
}