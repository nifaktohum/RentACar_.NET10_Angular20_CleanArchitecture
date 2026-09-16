using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Features.Vehicles.Dto;
using Application.Features.Vehicles.Specifications;
using Application.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetVehiclesByPriceRangeQuery : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetVehiclesByPriceRangeQueryHandler(
                        IVehicleRepository _vehicleRepo,
                        ILogger<GetVehiclesByPriceRangeQueryHandler> _logger
                    ) : IRequestHandler<GetVehiclesByPriceRangeQuery, Result<PagedResponse<VehicleListDto>>>
{
  public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetVehiclesByPriceRangeQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Fiyat aralığına göre araçlar getiriliyor. Min: {MinPrice}, Max: {MaxPrice}", _req.MinPrice, _req.MaxPrice);

      // 1. Specification nesnesini oluşturuyoruz
      var spec = new VehiclesByPriceRangeSpecification(_req);

      // 2. Kriter (Where) koşulunu uyguluyoruz
      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      // 3. Specification içerisindeki Include'ları dinamik olarak ekliyoruz
      foreach (var include in spec.Includes)
      {
        query = query.Include(include);
      }

      // Specification içinde belirlenen sıralama kuralını sorguya uyguluyoruz
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

      // 4. Veritabanına sorguyu gönder
      var totalCount = await query.CountAsync(_token);

      if (spec.IsPagingEnabled)
        query = query.Skip(spec.Skip).Take(spec.Take);

      // 5. Veritabanından asenkron olarak çekiyoruz
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

      _logger.LogInformation("✅ Fiyat filtreli {Count} araç getirildi", result.Count);

      var response = new PagedResponse<VehicleListDto>(
                      result,
                      totalCount,
                      _req.PageNumber,
                      _req.PageSize);

      return Result<PagedResponse<VehicleListDto>>.Succeed(response);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Fiyat aralığına göre araçlar getirilirken hata oluştu");
      return Result<PagedResponse<VehicleListDto>>.Failure(500, $"Araçlar getirilirken hata oluştu: {_ex.Message}");
    }
  }
}