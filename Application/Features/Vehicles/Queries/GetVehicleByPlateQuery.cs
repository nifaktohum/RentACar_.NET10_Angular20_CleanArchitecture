using Application.Features.Vehicles.Dto;
using Application.Features.Vehicles.Specifications;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

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
      _logger.LogInformation("Plaka sorgulaması yapılıyor. Plaka: {Plate}", _req.Plate);

      var spec = new VehicleByPlateSpecification(_req.Plate);

      // Adminin her şeyi görebilmesi için ilişkili tabloları (Include) ekliyoruz
      var vehicle = await _vehicleRepo.Where(spec.Criteria!)
                                      .AsNoTracking()
                                      .Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType)
                                      .Include(v => v.Images.Where(i => i.IsActive && !i.IsDeleted))
                                      .FirstOrDefaultAsync(_token);

      if (vehicle == null)
      {
        _logger.LogWarning("Sistemde bu plakaya ait bir kayıt bulunamadı! Plaka: {Plate}", _req.Plate);
        return Result<VehicleDetailDto>.Failure("Belirtilen plakaya ait araç bulunamadı.");
      }

      _logger.LogInformation("Araç başarıyla bulundu. Plaka: {Plate}", _req.Plate);

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
          vehicle.VehicleModel?.Name ?? "Belirtilmemiş",
          vehicle.VehicleModel?.VehicleType?.Name ?? "Belirtilmemiş",
          imageDtos,
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
      _logger.LogError(ex, "Plaka ile araç sorgulanırken sistemde bir hata oluştu! Plaka: {Plate}", _req.Plate);
      return Result<VehicleDetailDto>.Failure("Plaka sorgulaması sırasında bir hata oluştu.");
    }
  }
}