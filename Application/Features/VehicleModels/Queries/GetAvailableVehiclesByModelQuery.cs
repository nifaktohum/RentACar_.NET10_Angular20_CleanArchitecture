using Application.Common.Models;
using Application.Features.Vehicles.Dto;
using Application.Features.Vehicles.Specifications;
using Application.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleModels.Queries;

public sealed record GetAvailableVehiclesByModelQuery(string? Model) : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetAvailableVehiclesByModelQueryHandler(
    IVehicleRepository _vehicleRepo,
    ILogger<GetAvailableVehiclesByModelQueryHandler> _logger
) : IRequestHandler<GetAvailableVehiclesByModelQuery, Result<PagedResponse<VehicleListDto>>>
{
  public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetAvailableVehiclesByModelQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Müsait araçlar sorgulanıyor. Model Filtresi: {Model}", _req.Model ?? "Yok");

      // 1. Specification oluştur
      var spec = new AvailableVehicleByModelSpecification(_req.Model, _req);

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      // 2. Include'ları EKLE
      query = query
          .Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType)
          .Include(v => v.Images.Where(i => i.IsActive && !i.IsDeleted));

      // 3. Sıralama Mantığı (Varsayılan Sıralama else bloğunda)
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);
      else
        query = query.OrderBy(v => v.Brand).ThenBy(v => v.Model); // Varsayılan

      // 4. Toplam sayı ve Sayfalama
      var totalCount = await query.CountAsync(_token);

      if (spec.IsPagingEnabled)
        query = query.Skip(spec.Skip).Take(spec.Take);

      var vehicles = await query.ToListAsync(_token);

      _logger.LogInformation("✅ {Count} adet müsait araç başarıyla getirildi. Model Filtresi: {Model}", vehicles.Count, _req.Model ?? "Yok");

      // 5. Entity -> DTO Dönüşümü (Yeni alanlar eklendi)
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
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Araçlar modele göre getirilirken sistemde bir hata oluştu! Model Filtresi: {Model}", _req.Model ?? "Yok");

      return Result<PagedResponse<VehicleListDto>>.Failure("Araçlar modele göre getirilirken bir hata oluştu.");
    }
  }
}