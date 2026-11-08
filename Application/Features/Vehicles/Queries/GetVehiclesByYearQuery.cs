using Application.Features.Vehicles.Dto;
using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetVehiclesByYearQuery(int? Year) : IRequest<Result<List<VehicleListDto>>>;

public sealed class GetVehiclesByYearQueryHandler(
    IVehicleRepository _vehicleRepo,
    ILogger<GetVehiclesByYearQueryHandler> _logger
) : IRequestHandler<GetVehiclesByYearQuery, Result<List<VehicleListDto>>>
{
  public async Task<Result<List<VehicleListDto>>> Handle(GetVehiclesByYearQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Yıl bilgisine göre araçlar getiriliyor. Year: {Year}", _req.Year?.ToString() ?? "Tümü");

      var spec = new VehiclesByYearSpecification(_req.Year);

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      // Include'ları uygula
      foreach (var include in spec.Includes)
      {
        query = query.Include(include);
      }

      // 🆕 VehicleModel -> VehicleType ilişkisini dahil et (DTO'da tip adı dönebilmesi için)
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

      _logger.LogInformation("✅ Yıl filtreli {Count} araç getirildi. Year: {Year}", result.Count, _req.Year?.ToString() ?? "Tümü");

      return Result<List<VehicleListDto>>.Succeed(result);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Yıl bilgisine göre araçlar getirilirken hata oluştu. Year: {Year}", _req.Year);
      return Result<List<VehicleListDto>>.Failure(500, $"Araçlar getirilirken hata oluştu: {_ex.Message}");
    }
  }
}