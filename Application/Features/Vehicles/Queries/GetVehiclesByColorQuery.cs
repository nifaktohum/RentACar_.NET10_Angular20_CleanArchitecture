using Application.Features.Vehicles.Dto;
using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetVehiclesByColorQuery(string? Color) : IRequest<Result<List<VehicleListDto>>>;

public sealed class GetVehiclesByColorQueryHandler(
    IVehicleRepository _vehicleRepo,
    ILogger<GetVehiclesByColorQueryHandler> _logger
) : IRequestHandler<GetVehiclesByColorQuery, Result<List<VehicleListDto>>>
{
  public async Task<Result<List<VehicleListDto>>> Handle(GetVehiclesByColorQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Renk bilgisine göre araçlar getiriliyor. Color: {Color}", _req.Color ?? "Tümü");

      var spec = new VehiclesByColorSpecification(_req.Color);

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      foreach (var include in spec.Includes)
      {
        query = query.Include(include);
      }

      // 🆕 VehicleModel -> VehicleType ilişkisini dahil et
      query = query.Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType);

      var vehicles = await query.ToListAsync(_token);

      // 🆕 DTO Map'lemesi Güncellendi
      var result = vehicles.Select(v => new VehicleListDto(
          v.Id,
          v.Brand,
          v.Model,
          v.Description ?? "",
          v.Year,
          v.Plate,
          v.Color,
          v.VinNumber,
          v.FuelType,
          v.Transmission,
          v.TractionType,
          v.SeatCount,
          v.DoorCount,
          v.DailyPrice,
          v.CurrentKilometer,
          v.Status.ToString(),
          v.IsAvailable,
          v.VehicleModel?.VehicleType?.Name ?? "Bilinmiyor",
          v.GetMainImageUrl() ?? string.Empty,
          v.IsActive,
          v.CreatedAt
      )).ToList();

      _logger.LogInformation("✅ Renk filtreli {Count} araç getirildi. Color: {Color}", result.Count, _req.Color ?? "Tümü");

      return Result<List<VehicleListDto>>.Succeed(result);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Renk bilgisine göre araçlar getirilirken hata oluştu. Color: {Color}", _req.Color);
      return Result<List<VehicleListDto>>.Failure(500, $"Araçlar getirilirken hata oluştu: {_ex.Message}");
    }
  }
}