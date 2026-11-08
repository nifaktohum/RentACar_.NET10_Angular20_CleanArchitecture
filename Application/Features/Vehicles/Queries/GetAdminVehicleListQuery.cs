using Application.Features.Vehicles.Dto;
using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using Application.Specifications;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;
using Application.Common.Models;
using Application.Behaviors;

namespace Application.Features.Vehicles.Queries;

[Permission("Vehicles.Read")]
public sealed record GetAdminVehicleListQuery : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetAdminVehicleListQueryHandler(
    IVehicleRepository _vehicleRepo,
    ILogger<GetAdminVehicleListQueryHandler> _logger
) : IRequestHandler<GetAdminVehicleListQuery, Result<PagedResponse<VehicleListDto>>>
{
  public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetAdminVehicleListQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Admin paneli için araç listesi getiriliyor.");

      var spec = new AdminVehicleListSpecification(_req);

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      // Include işlemleri (VehicleModel ve VehicleType'ı çekmek zorundayız ki DTO'da dolabilsin)
      foreach (var include in spec.Includes)
      {
        query = query.Include(include);
      }

      // 🆕 VehicleModel -> VehicleType ilişkisini de dahil et
      // (Eğer Specification içinde zaten yapılmadıysa)
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

      // 🆕 YENİ ALANLAR EKLENDİ (Dto'yu da buna göre güncellemelisin)
      var result = vehicles.Select(v => new VehicleListDto(
          v.Id,
          v.Brand,
          v.Model,
          v.Description ?? "",
          v.Year,
          v.Plate,
          v.Color,
          v.VinNumber,               // ✅
          v.FuelType,
          v.Transmission,
          v.TractionType,            // ✅
          v.SeatCount,
          v.DoorCount,
          v.DailyPrice,
          v.CurrentKilometer,        // ✅
          v.Status.ToString(),       // ✅
          v.IsAvailable,             // ✅ (Status == Available && IsActive && !IsDeleted)
          v.VehicleModel?.VehicleType?.Name ?? "Bilinmiyor",
          v.GetMainImageUrl(),
          v.IsActive,
          v.CreatedAt
      )).ToList();

      _logger.LogInformation("✅ Admin paneli için {Count} araç başarıyla getirildi.", result.Count);

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
      _logger.LogError(_ex, "❌ Admin paneli araç listesi getirilirken hata oluştu.");
      return Result<PagedResponse<VehicleListDto>>.Failure(500, $"Araç listesi getirilirken hata oluştu: {_ex.Message}");
    }
  }
}