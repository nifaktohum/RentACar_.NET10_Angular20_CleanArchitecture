using Application.Features.Vehicles.Dto;
using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetVehicleStatisticsQuery : IRequest<Result<VehicleStatisticsDto>>;

public sealed class GetVehicleStatisticsQueryHandler(
                        IVehicleRepository _vehicleRepo,
                        ILogger<GetVehicleStatisticsQueryHandler> _logger
                    ) : IRequestHandler<GetVehicleStatisticsQuery, Result<VehicleStatisticsDto>>
{
  public async Task<Result<VehicleStatisticsDto>> Handle(GetVehicleStatisticsQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Araç istatistikleri hesaplanıyor.");

      var spec = new VehicleStatisticsSpecification();

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      var totalCount = await query.CountAsync(_token);
      var availableCount = await query.CountAsync(v => v.IsActive && v.IsAvailable, _token);
      var rentedCount = await query.CountAsync(v => v.IsActive && !v.IsAvailable, _token);
      var passiveCount = await query.CountAsync(v => !v.IsActive, _token);
      // Ortalama fiyat
      var avgPrice = totalCount > 0
          ? await query.AverageAsync(v => v.DailyPrice, _token)
          : 0;

      var statistics = new VehicleStatisticsDto(
          TotalVehicleCount: totalCount,
          AvailableVehicleCount: availableCount,
          RentedVehicleCount: rentedCount,
          PassiveVehicleCount: passiveCount,
          AverageDailyPrice: Math.Round(avgPrice, 2)
      );

      _logger.LogInformation("✅ Araç istatistikleri başarıyla hesaplandı.");

      return Result<VehicleStatisticsDto>.Succeed(statistics);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Araç istatistikleri hesaplanırken hata oluştu.");
      return Result<VehicleStatisticsDto>.Failure(500, $"İstatistikler hesaplanırken hata oluştu: {_ex.Message}");
    }
  }
}
