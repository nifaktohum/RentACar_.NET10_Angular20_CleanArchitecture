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

public sealed record GetVehiclesByTransmissionQuery(string? Transmission, VehicleSpecParams? SpecParams) : IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetVehiclesByTransmissionQueryHandler(
    IVehicleRepository _vehicleRepo,
    ILogger<GetVehiclesByTransmissionQueryHandler> _logger
) : IRequestHandler<GetVehiclesByTransmissionQuery, Result<PagedResponse<VehicleListDto>>>
{
  public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetVehiclesByTransmissionQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Vites tipine göre araçlar getiriliyor. Transmission: {Transmission}", _req.Transmission ?? "Tümü");

      var specParams = _req.SpecParams ?? new VehicleSpecParams();
      var spec = new VehiclesByTransmissionSpecification(_req.Transmission, specParams);

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      // Include'ları uygula
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

      // 🚨 Toplam sayıyı Sayfalama'dan ÖNCE alıyoruz
      var totalCount = await query.CountAsync(_token);

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
          v.GetMainImageUrl() ?? string.Empty,
          v.IsActive,
          v.CreatedAt
      )).ToList();

      _logger.LogInformation("✅ Vites tipi filtreli {Count} araç getirildi. Transmission: {Transmission}", result.Count, _req.Transmission ?? "Tümü");

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
      _logger.LogError(_ex, "❌ Vites tipine göre araçlar getirilirken hata oluştu. Transmission: {Transmission}", _req.Transmission);
      return Result<PagedResponse<VehicleListDto>>.Failure(500, $"Araçlar getirilirken hata oluştu: {_ex.Message}");
    }
  }
}