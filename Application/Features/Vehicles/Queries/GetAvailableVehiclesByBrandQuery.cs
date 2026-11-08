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

public sealed record GetAvailableVehiclesByBrandQuery(string? Brand) : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetAvailableVehiclesByBrandQueryHandler(
    IVehicleRepository _vehicleRepo,
    ILogger<GetAvailableVehiclesByBrandQueryHandler> _logger
) : IRequestHandler<GetAvailableVehiclesByBrandQuery, Result<PagedResponse<VehicleListDto>>>
{
  public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetAvailableVehiclesByBrandQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Müsait araçlar sorgulanıyor. Marka Filtresi: {Brand}", _req.Brand ?? "Tümü");

      // 1. Specification oluştur
      var spec = new AvailableVehiclesByBrandSpecification(_req.Brand, _req);

      // 2. Query'yi hazırla
      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      // 3. Include'ları EKLE
      query = query
          .Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType)
          .Include(v => v.Images.Where(i => i.IsActive && !i.IsDeleted));

      // Sıralama
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);
      else
        query = query.OrderBy(v => v.Brand).ThenBy(v => v.Model); // Varsayılan sıralama

      // 4. Toplam sayı ve Sayfalama
      var totalCount = await query.CountAsync(_token);
      if (spec.IsPagingEnabled)
        query = query.Skip(spec.Skip).Take(spec.Take);

      var vehicles = await query.ToListAsync(_token);

      _logger.LogInformation("✅ {Count} adet müsait araç başarıyla getirildi. Marka Filtresi: {Brand}", vehicles.Count, _req.Brand ?? "Tümü");

      // 5. Entity -> DTO dönüşümü (Yeni alanlar eklendi)
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
          v.GetMainImageUrl(),
          v.IsActive,
          v.CreatedAt
      )).ToList();

      var response = new PagedResponse<VehicleListDto>(
          vehicleDtos,
          totalCount,
          _req.PageNumber,
          _req.PageSize);

      return Result<PagedResponse<VehicleListDto>>.Succeed(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Araçlar markaya göre getirilirken sistemde bir hata oluştu! Marka Filtresi: {Brand}", _req.Brand ?? "Tümü");

      return Result<PagedResponse<VehicleListDto>>.Failure(
          "Araçlar markaya göre getirilirken bir hata oluştu."
      );
    }
  }
}