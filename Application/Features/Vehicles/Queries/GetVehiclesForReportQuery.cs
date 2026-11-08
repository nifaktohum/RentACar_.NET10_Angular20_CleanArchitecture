using Application.Common.Models;
using Application.Features.Vehicles.Dto;
using Application.Features.Vehicles.Specifications;
using Application.Specifications;
using Domain.Entities.Vehicles;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetVehiclesForReportQuery : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleReportDto>>>;

public sealed class GetVehiclesForReportQueryHandler(
    IVehicleRepository _vehicleRepo,
    ILogger<GetVehiclesForReportQueryHandler> _logger
) : IRequestHandler<GetVehiclesForReportQuery, Result<PagedResponse<VehicleReportDto>>>
{
  public async Task<Result<PagedResponse<VehicleReportDto>>> Handle(GetVehiclesForReportQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("📊 Rapor için araçlar getiriliyor...");

      var spec = new VehiclesForReportSpecification(_req);

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      // Include'ları uygula
      foreach (var include in spec.Includes)
      {
        query = query.Include(include);
      }

      // 🆕 VehicleModel -> VehicleType ilişkisini dahil et
      query = query.Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType);

      // 🚨 KRİTİK: VehicleModel.Stock hesaplanabilmesi için modelin altındaki araçları da çekmeliyiz!
      query = query.Include(v => v.VehicleModel).ThenInclude(vm => vm.Vehicles);

      // Sıralama
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

      // Toplam sayı
      var totalCount = await query.CountAsync(_token);

      if (spec.IsPagingEnabled)
        query = query.Skip(spec.Skip).Take(spec.Take);

      var vehicles = await query.ToListAsync(_token);

      // Map et (Yeni alanlar eklendi)
      var items = vehicles.Select(v => new VehicleReportDto(
          v.Id,
          v.Brand,
          v.Model,
          v.Year,
          v.Plate,
          v.VinNumber,                   // ✅
          v.Color,
          v.FuelType,
          v.Transmission,
          v.SeatCount,
          v.DoorCount,
          v.DailyPrice,
          v.CurrentKilometer,            // ✅
          v.VehicleModel.Stock,
          v.IsAvailable,
          v.VehicleModel.VehicleType?.Name ?? "Bilinmiyor",
          v.GetMainImageUrl() ?? string.Empty,
          v.IsActive,
          v.CreatedAt,
          v.UpdatedAt,
          GetStatus(v)
      )).ToList();

      var response = new PagedResponse<VehicleReportDto>(
          items,
          totalCount,
          _req.PageNumber,
          _req.PageSize
      );

      _logger.LogInformation("✅ Rapor için {Count} araç getirildi. Toplam: {TotalCount}", items.Count, totalCount);

      return Result<PagedResponse<VehicleReportDto>>.Succeed(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Rapor araçları getirilirken hata oluştu");
      return Result<PagedResponse<VehicleReportDto>>.Failure(500, $"Rapor araçları getirilirken hata oluştu: {ex.Message}");
    }
  }

  private static string GetStatus(Vehicle vehicle)
  {
    if (!vehicle.IsActive)
      return "Pasif";

    // 🆕 Enum kullanarak daha detaylı statü
    return vehicle.Status switch
    {
      VehicleStatus.Available => "Müsait",
      VehicleStatus.Rented => "Kirada",
      VehicleStatus.Maintenance => "Bakımda",
      VehicleStatus.OutOfService => "Serviste / Arızalı",
      _ => "Bilinmiyor"
    };
  }
}