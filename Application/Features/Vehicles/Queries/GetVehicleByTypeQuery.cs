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

public sealed record GetVehicleByTypeQuery(
    Guid VehicleTypeId,
    VehicleSpecParams? SpecParams
) : IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetVehicleByTypeQueryHandler(
    IVehicleRepository _vehicleRepo,
    ILogger<GetVehicleByTypeQueryHandler> _logger
) : IRequestHandler<GetVehicleByTypeQuery, Result<PagedResponse<VehicleListDto>>>
{
  public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetVehicleByTypeQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Belirtilen araç tipine ait araçlar getiriliyor. VehicleTypeId: {VehicleTypeId}", _req.VehicleTypeId);

      // SpecParams null ise varsayılan değerler kullan
      var specParams = _req.SpecParams ?? new VehicleSpecParams();

      var spec = new VehiclesByTypeSpecification(_req.VehicleTypeId, specParams);

      // Query'yi başlat
      var query = _vehicleRepo.GetAll();

      // Filtreleri uygula
      if (spec.Criteria != null)
      {
        query = query.Where(spec.Criteria);
      }

      // Include'ları uygula
      foreach (var include in spec.Includes)
      {
        query = query.Include(include);
      }

      // 🆕 VehicleModel -> VehicleType ilişkisini dahil et (DTO'da tip adı dönebilmesi için)
      query = query.Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType);

      // 🚨 KRİTİK: Toplam sayıyı SAYFALAMA YAPMADAN ÖNCE almalısın!
      var totalCount = await query.CountAsync(_token);

      // Sıralama uygula
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

      // Sayfalama uygula
      if (spec.IsPagingEnabled)
        query = query.Skip(spec.Skip).Take(spec.Take);

      // Verileri getir
      var vehicles = await query.ToListAsync(_token);

      // DTO'ya dönüştür (Yeni alanlar eklendi)
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
          v.GetMainImageUrl() ?? string.Empty,
          v.IsActive,
          v.CreatedAt
      )).ToList();

      _logger.LogInformation("✅ VehicleTypeId: {VehicleTypeId} için {Count} araç getirildi", _req.VehicleTypeId, result.Count);

      // PagedResponse oluştur
      var response = new PagedResponse<VehicleListDto>(
          result,
          totalCount,
          specParams.PageNumber,
          specParams.PageSize
      );

      return Result<PagedResponse<VehicleListDto>>.Succeed(response);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Araç tipine göre araçlar getirilirken hata oluştu. VehicleTypeId: {VehicleTypeId}", _req.VehicleTypeId);
      return Result<PagedResponse<VehicleListDto>>.Failure(500, $"Araçlar getirilirken hata oluştu: {_ex.Message}");
    }
  }
}