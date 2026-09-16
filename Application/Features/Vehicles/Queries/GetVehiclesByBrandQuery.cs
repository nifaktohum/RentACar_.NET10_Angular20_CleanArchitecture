using Application.Common.Models;
using Application.Features.Vehicles.Dto;
using Application.Features.Vehicles.Specifications;
using Application.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetVehiclesByBrandQuery(string? Brand) : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetVehiclesByBrandQueryHandler(
                        IVehicleRepository _vehicleRepo,
                        ILogger<GetVehiclesByBrandQueryHandler> _logger
                    ) : IRequestHandler<GetVehiclesByBrandQuery, Result<PagedResponse<VehicleListDto>>>
{
  public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetVehiclesByBrandQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Markaya göre araçlar getiriliyor. Brand: {Brand}", _req.Brand ?? "Tümü");

      var spec = new VehiclesByBrandSpecification(_req.Brand, _req);

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      foreach (var include in spec.Includes)
      {
        query = query.Include(include);
      }

      // Eğer marka parametresi verildiyse filtreleme ekleyelim (Case-insensitive arama için uygun yapılabilir)
      if (!string.IsNullOrWhiteSpace(_req.Brand))
      {
        query = query.Where(v => v.Brand.ToLower() == _req.Brand.ToLower());
      }

      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

      var totalCount = await query.CountAsync(_token);

      if (spec.IsPagingEnabled)
        query = query.Skip(spec.Skip).Take(spec.Take);

      var vehicles = await query.ToListAsync(_token);

      var vehicleDtos = vehicles.Select(v => new VehicleListDto(
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

      _logger.LogInformation("✅ Marka filtreli {Count} araç getirildi. Brand: {Brand}", vehicleDtos.Count, _req.Brand ?? "Tümü");

      var response = new PagedResponse<VehicleListDto>(
                            vehicleDtos,
                            totalCount,
                            _req.PageNumber,
                            _req.PageSize
      );

      return Result<PagedResponse<VehicleListDto>>.Succeed(response);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Markaya göre araçlar getirilirken hata oluştu. Brand: {Brand}", _req.Brand);
      return Result<PagedResponse<VehicleListDto>>.Failure(500, $"Araçlar getirilirken hata oluştu: {_ex.Message}");
    }
  }
}