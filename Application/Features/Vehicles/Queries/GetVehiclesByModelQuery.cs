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

public sealed record GetVehiclesByModelQuery(string? Model) : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetVehiclesByModelQueryHandler(
                        IVehicleRepository _vehicleRepo,
                        ILogger<GetVehiclesByModelQueryHandler> _logger
                    ) : IRequestHandler<GetVehiclesByModelQuery, Result<PagedResponse<VehicleListDto>>>
{
  public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetVehiclesByModelQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Modele göre araçlar getiriliyor. Model: {Model}", _req.Model ?? "Tümü");

      var spec = new VehiclesByModelSpecification(_req.Model, _req);

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      foreach (var include in spec.Includes)
      {
        query = query.Include(include);
      }

      // Eğer model parametresi verildiyse filtreleme ekleyelim (Case-insensitive)
      if (!string.IsNullOrWhiteSpace(_req.Model))
      {
        query = query.Where(v => v.Model.ToLower() == _req.Model.ToLower());
      }

      // Specification içinde belirlenen sıralama kuralını sorguya uyguluyoruz
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

      // Veritabanına sorguyu gönder
      var totalCount = await query.CountAsync(_token);
      if (spec.IsPagingEnabled)
        query = query.Skip(spec.Skip).Take(spec.Take);

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

      _logger.LogInformation("✅ Model filtreli {Count} araç getirildi. Model: {Model}", result.Count, _req.Model ?? "Tümü");

      var response = new PagedResponse<VehicleListDto>(
                            result,
                            totalCount,
                            _req.PageNumber,
                            _req.PageSize);
      return Result<PagedResponse<VehicleListDto>>.Succeed(response);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Modele göre araçlar getirilirken hata oluştu. Model: {Model}", _req.Model);
      return Result<PagedResponse<VehicleListDto>>.Failure(500, $"Araçlar getirilirken hata oluştu: {_ex.Message}");
    }
  }
}
