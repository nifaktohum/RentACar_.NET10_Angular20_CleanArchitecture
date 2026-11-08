using Application.Features.Vehicles.Dto;
using Application.Features.VehicleImages.Dto;
using Application.Features.Vehicles.Specifications;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetVehicleByIdQuery(Guid Id) : IRequest<Result<VehicleDetailDto>>;

public sealed class GetVehicleByIdQueryHandler(
    IVehicleRepository _vehicleRepo,
    IUserRepository _userRepo,
    ILogger<GetVehicleByIdQueryHandler> _logger
) : IRequestHandler<GetVehicleByIdQuery, Result<VehicleDetailDto>>
{
  public async Task<Result<VehicleDetailDto>> Handle(GetVehicleByIdQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Araç detayları sorgulanıyor. Araç ID: {VehicleId}", _req.Id);

      var spec = new VehicleByIdSpecification(_req.Id);

      var vehicle = await _vehicleRepo
          .Where(spec.Criteria!)
          .AsNoTracking()
          .Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType)
          .Include(v => v.Images.Where(i => i.IsActive && !i.IsDeleted))
          .FirstOrDefaultAsync(_token);

      if (vehicle == null)
      {
        _logger.LogWarning("❌ Araç bulunamadı veya yayında değil! Araç ID: {VehicleId}", _req.Id);
        return Result<VehicleDetailDto>.Failure("Belirtilen araç bulunamadı.");
      }

      _logger.LogInformation("✅ Araç detayı başarıyla getirildi. Araç ID: {VehicleId}", _req.Id);

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

      // Entity -> DTO Dönüşümü (Yeni alanlar eklendi)
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
      _logger.LogError(ex, "❌ Araç detayı getirilirken sistemde bir hata oluştu! Araç ID: {VehicleId}", _req.Id);
      return Result<VehicleDetailDto>.Failure("Araç detayları getirilirken bir hata oluştu.");
    }
  }
}