using Application.Features.Vehicles.Dto;
using Application.Features.VehicleImages.Dto;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TS.Result;
using Application.Behaviors;

namespace Application.Features.Vehicles.Commands;

[Permission("Vehicles.Update")]
public sealed record UpdateVehicleCommand(
    Guid Id,
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
    int LastMaintenanceKm,                 
    int NextMaintenanceKm,                 
    DateTimeOffset LastMaintenanceDate,    
    DateTimeOffset InspectionDate,         
    DateTimeOffset InsuranceEndDate,       
    DateTimeOffset CascoEndDate,           
    string TireStatus,                     
    string GeneralStatus,                  

    string? Description,
    bool IsActive
) : IRequest<Result<UpdateVehicleDto>>;


public sealed class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
  public UpdateVehicleCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty().WithMessage("Araç ID boş olamaz!");

    // Temel
    RuleFor(x => x.Brand).NotEmpty().MaximumLength(50);
    RuleFor(x => x.Model).NotEmpty().MaximumLength(50);
    RuleFor(x => x.Year).NotEmpty().Length(4)
        .Must(y => int.TryParse(y, out var year) && year >= 1900 && year <= 2100)
        .WithMessage("Yıl 1900 ile 2100 arasında olmalı!");
    RuleFor(x => x.Plate).NotEmpty().MaximumLength(20).Matches(@"^[A-Z0-9\s-]+$");
    RuleFor(x => x.VehicleModelId).NotEmpty();
    RuleFor(x => x.BranchId).NotEmpty().WithMessage("Şube seçilmelidir!");

    // Tanımlayıcı
    RuleFor(x => x.VinNumber).NotEmpty().Length(17).WithMessage("Şasi numarası 17 karakter olmalıdır!");
    RuleFor(x => x.EngineNumber).NotEmpty().MaximumLength(30);

    // Teknik
    RuleFor(x => x.FuelType).NotEmpty().MaximumLength(20);
    RuleFor(x => x.Transmission).NotEmpty().MaximumLength(20);
    RuleFor(x => x.TractionType).NotEmpty().MaximumLength(20);
    RuleFor(x => x.EngineVolume).GreaterThan(0);
    RuleFor(x => x.EnginePower).GreaterThan(0);
    RuleFor(x => x.FuelConsumption).GreaterThan(0);
    RuleFor(x => x.SeatCount).InclusiveBetween(1, 60);
    RuleFor(x => x.DoorCount).InclusiveBetween(1, 10);

    // Ticari
    RuleFor(x => x.MinAge).InclusiveBetween(18, 80).When(x => x.MinAge.HasValue);
    RuleFor(x => x.DailyPrice).GreaterThan(0);
    RuleFor(x => x.DepositAmount).GreaterThanOrEqualTo(0);
    RuleFor(x => x.WeeklyDiscountRate).InclusiveBetween(0, 100);
    RuleFor(x => x.MonthlyDiscountRate).InclusiveBetween(0, 100);

    // Bakım
    RuleFor(x => x.LastMaintenanceKm).GreaterThanOrEqualTo(0);
    RuleFor(x => x.NextMaintenanceKm).GreaterThan(x => x.LastMaintenanceKm)
        .WithMessage("Sonraki bakım km'si son bakımdan büyük olmalı!");
  }
}


public sealed class UpdateVehicleCommandHandler(
    IVehicleRepository _vehicleRepo,
    IVehicleModelRepository _vehicleModelRepo,
    IUserRepository _userRepo,
    IUnitOfWork _unit,
    IConfiguration _config,
    ILogger<UpdateVehicleCommandHandler> _logger
) : IRequestHandler<UpdateVehicleCommand, Result<UpdateVehicleDto>>
{
  public async Task<Result<UpdateVehicleDto>> Handle(UpdateVehicleCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("Araç güncelleme başladı. VehicleId: {VehicleId}", _req.Id);

      // 1. Vehicle var mı kontrol et
      var vehicle = await _vehicleRepo.GetVehicleWithDetailsAsync(_req.Id, _token);

      if (vehicle is null)
      {
        _logger.LogWarning("❌ Araç bulunamadı. VehicleId: {VehicleId}", _req.Id);
        return Result<UpdateVehicleDto>.Failure(404, "Araç bulunamadı!");
      }

      // 2. Plate unique mi kontrol et (kendi plakası hariç)
      var isPlateUnique = await _vehicleRepo.IsPlateUniqueAsync(_req.Plate, _req.Id, _token);
      if (!isPlateUnique)
      {
        _logger.LogWarning("❌ Plaka zaten mevcut. Plate: {Plate}", _req.Plate);
        return Result<UpdateVehicleDto>.Failure(409, $"'{_req.Plate}' plakalı araç zaten mevcut!");
      }

      // 🆕 3. VIN unique mi kontrol et (Repository'de varsa)
      // var isVinUnique = await _vehicleRepo.IsVinUniqueAsync(_req.VinNumber, _req.Id, _token);
      // if (!isVinUnique)
      //     return Result<UpdateVehicleDto>.Failure(409, $"'{_req.VinNumber}' şasi numaralı araç zaten mevcut!");

      // 4. VehicleModel var mı kontrol et
      var vehicleModel = await _vehicleModelRepo.FirstOrDefaultAsync(x => x.Id == _req.VehicleModelId && !x.IsDeleted, _token);
      if (vehicleModel is null)
      {
        _logger.LogWarning("❌ Araç Model bulunamadı. VehicleModelId: {VehicleModelId}", _req.VehicleModelId);
        return Result<UpdateVehicleDto>.Failure(404, "Araç Model bulunamadı!");
      }

      // 5. Vehicle'ı güncelle (YENİ PARAMETRELER EKLENDİ)
      _logger.LogInformation("📝 Araç güncelleniyor...");

      vehicle.UpdateDetails(
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
          lastMaintenanceKm: _req.LastMaintenanceKm,
          nextMaintenanceKm: _req.NextMaintenanceKm,
          lastMaintenanceDate: _req.LastMaintenanceDate,
          inspectionDate: _req.InspectionDate,
          insuranceEndDate: _req.InsuranceEndDate,
          cascoEndDate: _req.CascoEndDate,
          tireStatus: _req.TireStatus,
          generalStatus: _req.GeneralStatus,
          description: _req.Description
      );

      // 6. IsActive durumunu güncelle
      if (_req.IsActive) { vehicle.Activate(); }
      else { vehicle.Deactivate(); }

      // 7. Güncelleme metadata'sını set et
      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);
      vehicle.UpdateMetadata(userId);

      // 8. Save
      await _unit.SaveChangesAsync(_token);

      _logger.LogInformation("✅ Araç başarıyla güncellendi. VehicleId: {VehicleId}", _req.Id);

      var userName = await _userRepo.GetUserNamesByIdsAsync(new List<Guid> { userId }, _token);
      string GetUserName(Guid id) => userName.GetValueOrDefault(id, "Bilinmiyor");

      // 9. Response oluştur
      var response = new UpdateVehicleDto(
          Id: vehicle.Id,
          Brand: vehicle.Brand,
          Model: vehicle.Model,
          Year: vehicle.Year,
          Plate: vehicle.Plate,
          Color: vehicle.Color,
          VehicleModelId: vehicle.VehicleModelId,
          VehicleModelName: vehicleModel.Name,
          VehicleTypeName: "Belirtilmemiş", // İhtiyaç varsa vehicleModel.VehicleType.Name çekilebilir
          BranchId: vehicle.BranchId,
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
          MinAge: vehicle.MinAge,
          DailyPrice: vehicle.DailyPrice,
          DepositAmount: vehicle.DepositAmount,
          WeeklyDiscountRate: vehicle.WeeklyDiscountRate,
          MonthlyDiscountRate: vehicle.MonthlyDiscountRate,
          CurrentKilometer: vehicle.CurrentKilometer,
          LastMaintenanceKm: vehicle.LastMaintenanceKm,
          NextMaintenanceKm: vehicle.NextMaintenanceKm,
          LastMaintenanceDate: vehicle.LastMaintenanceDate,
          InspectionDate: vehicle.InspectionDate,
          InsuranceEndDate: vehicle.InsuranceEndDate,
          CascoEndDate: vehicle.CascoEndDate,
          TireStatus: vehicle.TireStatus,
          GeneralStatus: vehicle.GeneralStatus,
          IsAvailable: vehicle.IsAvailable,
          Status: vehicle.Status.ToString(),
          IsActive: vehicle.IsActive,
          Description: vehicle.Description,
          MainImageUrl: vehicle.GetMainImageUrl(), // 🆕 Entity'deki metodu kullanıyoruz
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
          CreatedAt: vehicle.CreatedAt,
          CreatedBy: vehicle.CreatedBy,
          CreatedByName: GetUserName(vehicle.CreatedBy),
          UpdatedAt: vehicle.UpdatedAt,
          UpdatedBy: vehicle.UpdatedBy,
          UpdatedByName: vehicle.UpdatedBy.HasValue ? GetUserName(vehicle.UpdatedBy.Value) : null
      );

      return Result<UpdateVehicleDto>.Succeed(response);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Araç güncelleme hatası. VehicleId: {VehicleId}", _req.Id);
      return Result<UpdateVehicleDto>.Failure(500, $"Araç güncellenirken bir hata oluştu: {_ex.Message}");
    }
  }
}