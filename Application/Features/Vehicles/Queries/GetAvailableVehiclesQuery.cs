using Application.Features.Vehicles.Dto;
using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using Application.Specifications;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;
using Application.Common.Models;

namespace Application.Features.Vehicles.Queries;

// public sealed record GetAvailableVehiclesQuery(VehicleSpecParams? Filters, string? Sort): IRequest<Result<List<VehicleListDto>>>;
public sealed record GetAvailableVehiclesQuery:VehicleSpecParams, IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetAvailableVehiclesQueryHandler(
                        IVehicleRepository _vehicleRepo,
                        ILogger<GetAvailableVehiclesQueryHandler> _logger
                    ) : IRequestHandler<GetAvailableVehiclesQuery, Result<PagedResponse<VehicleListDto>>>
{
  public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetAvailableVehiclesQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("Müsait araçlar getiriliyor...");

      // 1. Specification oluştur
      var spec = new AvailableVehiclesSpecification(_req);

      // 2. Repository'den IQueryable al - Where metodu IQueryable dönüyor ✅
      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)      // IQueryable döner
          : _vehicleRepo.GetAll();                  // IQueryable döner

      // 3. Include'ları ekle (EF Core Include desteği)
      query = query
          .Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType)
          .Include(v => v.Images.Where(i => i.IsActive && !i.IsDeleted));

      // Specification içinde belirlenen sıralama kuralını sorguya uyguluyoruz
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

      var totalCount = await query.CountAsync(_token);

      if (spec.IsPagingEnabled)
          query = query.Skip(spec.Skip).Take(spec.Take);
      // 5. Veritabanına sorguyu gönder
      var vehicles = await query.ToListAsync(_token);

      _logger.LogInformation($"{vehicles.Count} adet müsait araç başarıyla getirildi");

      // 6. Mapping
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
          v.VehicleModel?.VehicleType?.Name ?? "Belirtilmemiş",
          v.GetMainImageUrl(),
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
      _logger.LogError(ex, "Müsait araçlar getirilirken bir hata oluştu!");
      return Result<PagedResponse<VehicleListDto>>.Failure("Müsait araçlar getirilirken beklenmeyen bir hata oluştu.");
    }
  }
}

