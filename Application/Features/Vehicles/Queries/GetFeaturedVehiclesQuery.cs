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
// specParams.Sort alanı "latest" değerini alacak, extension metod bunu yakalayacak ve 
// CreatedAt alanına göre azalan (en yeniler en başta olacak şekilde) sıralama yapıp latest sorgusunu kusursuzca çalıştır
public sealed record GetFeaturedVehiclesQuery : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetFeaturedVehiclesQueryHandler(
                        IVehicleRepository _vehicleRepo,
                        ILogger<GetFeaturedVehiclesQueryHandler> _logger
                    ) : IRequestHandler<GetFeaturedVehiclesQuery, Result<PagedResponse<VehicleListDto>>>
{
  public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetFeaturedVehiclesQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Öne çıkan araçlar getiriliyor.");

      var spec = new FeaturedVehiclesSpecification(_req);

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      foreach (var include in spec.Includes)
      {
        query = query.Include(include);
      }

      // Specification içinde belirlenen sıralama kuralını sorguya uyguluyoruz
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

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

      _logger.LogInformation("✅ Öne çıkan {Count} araç başarıyla getirildi.", result.Count);

      var response = new PagedResponse<VehicleListDto>(
                         result,
                         totalCount,
                         _req.PageNumber,
                         _req.PageSize);

      return Result<PagedResponse<VehicleListDto>>.Succeed(response);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Öne çıkan araçlar getirilirken hata oluştu.");
      return Result<PagedResponse<VehicleListDto>>.Failure(500, $"Öne çıkan araçlar getirilirken hata oluştu: {_ex.Message}");
    }
  }
}