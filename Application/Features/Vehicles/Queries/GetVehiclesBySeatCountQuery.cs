using Application.Features.Vehicles.Dto;
using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetVehiclesBySeatCountQuery(int? SeatCount) : IRequest<Result<List<VehicleListDto>>>;

public sealed class GetVehiclesBySeatCountQueryHandler(
                        IVehicleRepository _vehicleRepo,
                        ILogger<GetVehiclesBySeatCountQueryHandler> _logger
                    ) : IRequestHandler<GetVehiclesBySeatCountQuery, Result<List<VehicleListDto>>>
{
  public async Task<Result<List<VehicleListDto>>> Handle(GetVehiclesBySeatCountQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Koltuk sayısına göre araçlar getiriliyor. SeatCount: {SeatCount}", _req.SeatCount?.ToString() ?? "Tümü");

      var spec = new VehiclesBySeatCountSpecification(_req.SeatCount);

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      foreach (var include in spec.Includes)
      {
        query = query.Include(include);
      }

      var vehicles = await query.ToListAsync(_token);

      var result = vehicles.Select(v => new VehicleListDto(
          v.Id,
          v.Brand,
          v.Model,
          v.Description ?? "",
          v.Year,
          v.Plate,
          v.Color,
          v.FuelType,
          v.Transmission,
          v.SeatCount,
          v.DoorCount,
          v.DailyPrice,
          v.IsAvailable,
          v.VehicleModel?.VehicleType?.Name ?? "Bilinmiyor",
          v.GetMainImageUrl(),
          v.IsActive,
          v.CreatedAt
      )).ToList();

      _logger.LogInformation("✅ Koltuk sayısı filtreli {Count} araç getirildi. SeatCount: {SeatCount}", result.Count, _req.SeatCount?.ToString() ?? "Tümü");

      return Result<List<VehicleListDto>>.Succeed(result);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Koltuk sayısına göre araçlar getirilirken hata oluştu. SeatCount: {SeatCount}", _req.SeatCount);
      return Result<List<VehicleListDto>>.Failure(500, $"Araçlar getirilirken hata oluştu: {_ex.Message}");
    }
  }
}
