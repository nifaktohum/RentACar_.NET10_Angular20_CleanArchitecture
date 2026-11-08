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

      var spec = new VehiclesByPriceRangeSpecification(_req);

      // 1. Kriter (Where) koşulunu uygula
      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      // 2. Specification içerisindeki Include'ları ekle
      foreach (var include in spec.Includes)
      {
        query = query.Include(include);
      }

      // 🆕 VehicleModel -> VehicleType ilişkisini dahil et (DTO'da tip adı dönebilmesi için)
      query = query.Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType);

      // Sıralama
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

      // 🚨 Toplam sayıyı Sayfalama'dan ÖNCE al
      var totalCount = await query.CountAsync(_token);

      if (spec.IsPagingEnabled)
        query = query.Skip(spec.Skip).Take(spec.Take);

      // 3. Veritabanından asenkron olarak çek
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
          v.GetMainImageUrl() ?? string.Empty,
          v.IsActive,
          v.CreatedAt
      )).ToList();

      _logger.LogInformation("✅ Fiyat filtreli {Count} araç getirildi", result.Count);

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
      _logger.LogError(_ex, "❌ Fiyat aralığına göre araçlar getirilirken hata oluştu");
      return Result<PagedResponse<VehicleListDto>>.Failure(500, $"Araçlar getirilirken hata oluştu: {_ex.Message}");
    }
  }
}