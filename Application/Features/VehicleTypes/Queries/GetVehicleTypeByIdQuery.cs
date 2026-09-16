using Application.Features.VehicleTypes.Dto;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleTypes.Queries;

public sealed record GetVehicleTypeByIdQuery(Guid Id) : IRequest<Result<VehicleTypeDetailDto>>;


public sealed class GetVehicleTypeByIdQueryHandler(
                        IVehicleTypeRepository _vehicleTypeRepo,
                        IUserRepository _userRepo,
                        ILogger<GetVehicleTypeByIdQueryHandler> _logger
                    ) : IRequestHandler<GetVehicleTypeByIdQuery, Result<VehicleTypeDetailDto>>
{
  public async Task<Result<VehicleTypeDetailDto>> Handle(GetVehicleTypeByIdQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Araç tipi detayı getiriliyor: {Id}", _req.Id);

      var vehicleType = await _vehicleTypeRepo.GetByExpressionAsync(v => v.Id == _req.Id && !v.IsDeleted, _token);
      if (vehicleType is null) return Result<VehicleTypeDetailDto>.Failure("Araç Tipi bulunamadı!");


      var userId = new Guid(vehicleType.CreatedBy.ToString());
      var userName = await _userRepo.GetUserNamesByIdsAsync([userId], _token);

      string GetUserName(Guid userId) => userName.GetValueOrDefault(userId, "Bilinmiyor");

      // SQL tarafında AsNoTracking ve Projeksiyon kullanımı
      // 1. Veritabanından ham verileri SQL seviyesinde çekiyoruz
      var rawData = await _vehicleTypeRepo
          .GetAll()
          .AsNoTracking()
          .Where(vt => vt.Id == _req.Id && !vt.IsDeleted)
          .Select(v => new
          {
            v.Id,
            v.Name,
            v.Description,
            v.Icon,
            v.DisplayOrder,
            v.IsActive,
            VehicleModelCount = v.VehicleModels.Count(vm => !vm.IsDeleted),
            v.CreatedAt,
            v.CreatedBy,
            v.UpdatedAt,
            v.UpdatedBy
          })
          .FirstOrDefaultAsync(_token);

      if (rawData is null)
      {
        return Result<VehicleTypeDetailDto>.Failure(404, "Araç tipi bulunamadı.");
      }

      // 2. C# Belleğinde (In-Memory) C# metodlarını çağırarak DTO'yu oluşturuyoruz
      var response = new VehicleTypeDetailDto(
          rawData.Id,
          rawData.Name,
          rawData.Description,
          rawData.Icon,
          rawData.DisplayOrder,
          rawData.IsActive,
          rawData.VehicleModelCount,
          rawData.CreatedAt,
          rawData.CreatedBy,
          GetUserName(rawData.CreatedBy),
          rawData.UpdatedAt,
          rawData.UpdatedBy,
          rawData.UpdatedBy.HasValue ? GetUserName(rawData.UpdatedBy.Value) : null
      );


      _logger.LogInformation("✅ ID: {Id} olan araç tipi getirildi.", _req.Id);
      return Result<VehicleTypeDetailDto>.Succeed(response);

    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ ID: {Id} olan araç tipi getirilirken hata oluştu", _req.Id);
      return Result<VehicleTypeDetailDto>.Failure(500, $"Araç tipi detayları getirilirken hata oluştu: {_ex.Message}");
    }
  }
}
