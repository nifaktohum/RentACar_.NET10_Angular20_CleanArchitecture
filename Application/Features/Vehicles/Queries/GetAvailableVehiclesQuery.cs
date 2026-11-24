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

public sealed record GetAvailableVehiclesQuery : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetAvailableVehiclesQueryHandler(
    IVehicleRepository _vehicleRepo,
    ILogger<GetAvailableVehiclesQueryHandler> _logger
) : IRequestHandler<GetAvailableVehiclesQuery, Result<PagedResponse<VehicleListDto>>>
{
  public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetAvailableVehiclesQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Müsait araçlar getiriliyor...");

      // 1. Specification (Criteria + Include + Distinct + Sorting + Paging)
      var spec = new AvailableVehiclesSpecification(_req);

      // 2. Base query
      var query = _vehicleRepo.Where(spec.Criteria).AsNoTracking();

      // 3. Include'ları specification'dan uygula
      foreach (var include in spec.Includes)
        query = query.Include(include);

      // 4. Distinct (Images include'u duplicate getirebilir)
      if (spec.IsDistinct)
        query = query.Distinct();

      _logger.LogInformation("🔍 Üretilen SQL: {Sql}", query.ToQueryString());

      _logger.LogInformation("🔍 Start={Start} End={End}", _req.StartDate, _req.EndDate);

      // 5. Toplam sayı (sayfalamadan ÖNCE, Distinct SONRASI)
      var totalCount = await query.CountAsync(_token);

      // 6. Sıralama
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);
      else if (spec.ThenBy != null)
        query = ((IOrderedQueryable<Vehicle>)query).ThenBy(spec.ThenBy);
      else if (spec.ThenByDescending != null)
        query = ((IOrderedQueryable<Vehicle>)query).ThenByDescending(spec.ThenByDescending);

      // 7. Sayfalama
      if (spec.IsPagingEnabled)
        query = query.Skip(spec.Skip).Take(spec.Take);

      // 8. Veriyi çek
      var vehicles = await query.ToListAsync(_token);

      _logger.LogInformation("✅ {Count} adet müsait araç getirildi (total: {Total}).", vehicles.Count, totalCount);

      // 9. Mapping (bellekte — domain metotları sorunsuz çalışır)
      var vehicleDtos = vehicles.Select(v => new VehicleListDto(
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
          v.VehicleModel?.VehicleType?.Name ?? "Belirtilmemiş",
          v.GetMainImageUrl() ?? string.Empty,
          v.IsActive,
          v.CreatedAt
      )).ToList();

      var response = new PagedResponse<VehicleListDto>(
          vehicleDtos,
          totalCount,
          _req.PageNumber,
          _req.PageSize
      );

      return Result<PagedResponse<VehicleListDto>>.Succeed(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Müsait araçlar getirilirken hata oluştu.");
      // Kullanıcıya iç detay sızdırma
      return Result<PagedResponse<VehicleListDto>>.Failure("Müsait araçlar getirilirken bir hata oluştu.");
    }
  }
}