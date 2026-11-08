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

public sealed record GetRecommendedVehiclesQuery : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetRecommendedVehiclesQueryHandler(
    IVehicleRepository _vehicleRepo,
    ILogger<GetRecommendedVehiclesQueryHandler> _logger
) : IRequestHandler<GetRecommendedVehiclesQuery, Result<PagedResponse<VehicleListDto>>>
{
  public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetRecommendedVehiclesQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Tavsiye edilen araçlar getiriliyor.");

      var spec = new RecommendedVehiclesSpecification(_req);

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      foreach (var include in spec.Includes)
      {
        query = query.Include(include);
      }

      // 🆕 VehicleModel -> VehicleType ilişkisini dahil et
      query = query.Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType);

      // Sıralama
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

      // Toplam sayı
      var totalCount = await query.CountAsync(_token);

      // Sayfalama
      if (spec.IsPagingEnabled)
        query = query.Skip(spec.Skip).Take(spec.Take);

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
          v.GetMainImageUrl(),
          v.IsActive,
          v.CreatedAt
      )).ToList();

      _logger.LogInformation("✅ Tavsiye edilen {Count} araç başarıyla getirildi.", result.Count);

      var response = new PagedResponse<VehicleListDto>(
          result,
          totalCount,
          _req.PageNumber,
          _req.PageSize
      );

      return Result<PagedResponse<VehicleListDto>>.Succeed(response);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Tavsiye edilen araçlar getirilirken hata oluştu.");
      return Result<PagedResponse<VehicleListDto>>.Failure(500, $"Tavsiye edilen araçlar getirilirken hata oluştu: {_ex.Message}");
    }
  }
}