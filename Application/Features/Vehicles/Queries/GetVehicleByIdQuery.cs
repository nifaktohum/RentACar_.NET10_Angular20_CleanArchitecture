using Application.Features.Vehicles.Dto;
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
      _logger.LogInformation("Araç detayları sorgulanıyor. Araç ID: {VehicleId}", _req.Id);

      var spec = new VehicleByIdSpecification(_req.Id);

      // Generic repo'nun Where metodunu kullanıyoruz ve üzerine EF Core Include'larını ekliyoruz.
      // FirstOrDefaultAsync ile tek bir kayıt çekiyoruz.
      var vehicle = await _vehicleRepo
               .Where(spec.Criteria!)
               .AsNoTracking()
               .Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType)
               .Include(v => v.Images.Where(i => i.IsActive && !i.IsDeleted))
               .FirstOrDefaultAsync(_token);

      // Araç veritabanında yoksa veya pasif/silinmiş durumdaysa
      if (vehicle == null)
      {
        _logger.LogWarning("Araç bulunamadı veya yayında değil! Araç ID: {VehicleId}", _req.Id);
        return Result<VehicleDetailDto>.Failure("Belirtilen araç bulunamadı.");
      }

      _logger.LogInformation("Araç detayı başarıyla getirildi. Araç ID: {VehicleId}", _req.Id);

      var userId = new Guid(vehicle.CreatedBy.ToString());
      var userName = await _userRepo.GetUserNamesByIdsAsync([userId], _token);

      string GetUserName(Guid userId) => userName.GetValueOrDefault(userId, "Bilinmiyor");

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

      // Entity -> DTO Dönüşümü
      var vehicleDto = new VehicleDetailDto(
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
          vehicle.VehicleModel.Name,
          vehicle.VehicleModel.VehicleType?.Name ?? "Belirtilmemiş",
          imageDtos, // Senin yazdığın o harika Domain metodunu kullanıyoruz
          vehicle.IsActive,
          vehicle.CreatedAt,
          vehicle.CreatedBy,
          GetUserName(vehicle.CreatedBy),
          vehicle.UpdatedAt,
          vehicle.UpdatedBy,
          vehicle.UpdatedBy.HasValue ? GetUserName(vehicle.UpdatedBy.Value) : null
      );

      return Result<VehicleDetailDto>.Succeed(vehicleDto);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Araç detayı getirilirken sistemde bir hata oluştu! Araç ID: {VehicleId}", _req.Id);
      return Result<VehicleDetailDto>.Failure("Araç detayları getirilirken bir hata oluştu.");
    }
  }
}
