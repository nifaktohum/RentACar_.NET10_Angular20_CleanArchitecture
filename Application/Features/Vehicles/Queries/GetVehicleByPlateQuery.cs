using Application.Features.Vehicles.Dto;
using Application.Features.VehicleImages.Dto;
using Application.Features.Vehicles.Specifications;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;
using Application.Behaviors;

namespace Application.Features.Vehicles.Queries;

[Permission("Vehicles.Read")]
public sealed record GetVehicleByPlateQuery(string Plate) : IRequest<Result<VehicleDetailDto>>;

public sealed class GetVehicleByPlateQueryHandler(
    IVehicleRepository _vehicleRepo,
    IUserRepository _userRepo,
    ILogger<GetVehicleByPlateQueryHandler> _logger
) : IRequestHandler<GetVehicleByPlateQuery, Result<VehicleDetailDto>>
{
  public async Task<Result<VehicleDetailDto>> Handle(GetVehicleByPlateQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Plaka sorgulaması yapılıyor. Plaka: {Plate}", _req.Plate);

      var spec = new VehicleByPlateSpecification(_req.Plate);

      // Adminin her şeyi görebilmesi için ilişkili tabloları (Include) ekliyoruz
      var vehicle = await _vehicleRepo
          .Where(spec.Criteria!)
          .AsNoTracking()
          .Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType)
          .Include(v => v.Images.Where(i => i.IsActive && !i.IsDeleted))
          .FirstOrDefaultAsync(_token);

      if (vehicle == null)
      {
        _logger.LogWarning("❌ Sistemde bu plakaya ait bir kayıt bulunamadı! Plaka: {Plate}", _req.Plate);
        return Result<VehicleDetailDto>.Failure("Belirtilen plakaya ait araç bulunamadı.");
      }

      _logger.LogInformation("✅ Araç başarıyla bulundu. Plaka: {Plate}", _req.Plate);

      var userId = vehicle.CreatedBy;
      var userName = await _userRepo.GetUserNamesByIdsAsync([userId], _token);

      string GetUserName(Guid id) => userName.GetValueOrDefault(id, "Bilinmiyor");

      var imageDtos = vehicle.Images
          .Where(i => i.IsActive && !i.IsDeleted)
          .OrderBy(i => i.DisplayOrder)
          .Select(i => new VehicleImageDto(
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
          ))
          .ToList();

      // Entity -> DTO Dönüşümü (DTO sırasına tam uyumlu)
      var vehicleDto = new VehicleDetailDto(
          Id: vehicle.Id,

          // --- Temel Bilgiler ---
          Brand: vehicle.Brand,
          Model: vehicle.Model,
          Year: vehicle.Year,
          Plate: vehicle.Plate,
          Color: vehicle.Color,
          VehicleModelId: vehicle.VehicleModelId,
          VehicleModelName: vehicle.VehicleModel?.Name ?? "Belirtilmemiş",
          VehicleTypeName: vehicle.VehicleModel?.VehicleType?.Name ?? "Belirtilmemiş",
          BranchId: vehicle.BranchId,

          // --- Teknik & Tanımlayıcı Bilgiler ---
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

          // --- Ticari & Yasal Bilgiler ---
          MinAge: vehicle.MinAge,
          DailyPrice: vehicle.DailyPrice,
          DepositAmount: vehicle.DepositAmount,
          WeeklyDiscountRate: vehicle.WeeklyDiscountRate,
          MonthlyDiscountRate: vehicle.MonthlyDiscountRate,

          // --- Bakım & Durum Bilgileri ---
          CurrentKilometer: vehicle.CurrentKilometer,
          LastMaintenanceKm: vehicle.LastMaintenanceKm,
          NextMaintenanceKm: vehicle.NextMaintenanceKm,
          LastMaintenanceDate: vehicle.LastMaintenanceDate,
          InspectionDate: vehicle.InspectionDate,
          InsuranceEndDate: vehicle.InsuranceEndDate,
          CascoEndDate: vehicle.CascoEndDate,
          TireStatus: vehicle.TireStatus,
          GeneralStatus: vehicle.GeneralStatus,

          // --- Sistem & Durum ---
          IsAvailable: vehicle.IsAvailable,
          Status: vehicle.Status.ToString(),
          IsActive: vehicle.IsActive,
          Description: vehicle.Description,
          Images: imageDtos,

          // --- Audit Bilgileri ---
          CreatedAt: vehicle.CreatedAt,
          CreatedBy: vehicle.CreatedBy,
          CreatedByName: GetUserName(vehicle.CreatedBy),
          UpdatedAt: vehicle.UpdatedAt,
          UpdatedBy: vehicle.UpdatedBy,
          UpdatedByName: vehicle.UpdatedBy.HasValue ? GetUserName(vehicle.UpdatedBy.Value) : null
      );

      return Result<VehicleDetailDto>.Succeed(vehicleDto);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Plaka ile araç sorgulanırken sistemde bir hata oluştu! Plaka: {Plate}", _req.Plate);
      return Result<VehicleDetailDto>.Failure("Plaka sorgulaması sırasında bir hata oluştu.");
    }
  }
}