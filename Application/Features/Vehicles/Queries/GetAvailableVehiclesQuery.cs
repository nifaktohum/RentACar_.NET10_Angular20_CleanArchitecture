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

public sealed record GetAvailableVehiclesQuery : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetAvailableVehiclesQueryHandler(
    IVehicleRepository _vehicleRepo,
    ILogger<GetAvailableVehiclesQueryHandler> _logger
) : IRequestHandler<GetAvailableVehiclesQuery, Result<PagedResponse<VehicleListDto>>>
{
  public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetAvailableVehiclesQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Müsait araçlar getiriliyor...");

      // 1. Specification oluştur
      var spec = new AvailableVehiclesSpecification(_req);

      // 2. Query'yi hazırla
      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      // 3. Include'ları ekle (AsNoTracking ile performansı artırıyoruz)
      query = query
          .AsNoTracking()
          .Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType)
          .Include(v => v.Images.Where(i => i.IsActive && !i.IsDeleted));

      // 4. Sıralama (Sıralamayı Include'den sonra yapıyoruz)
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

      // 5. Toplam sayı (Sayfalamadan ÖNCE)
      var totalCount = await query.CountAsync(_token);

      // 6. Sayfalama
      if (spec.IsPagingEnabled)
        query = query.Skip(spec.Skip).Take(spec.Take);

      // 7. Veriyi veritabanından çek (Belleğe alıyoruz)
      var vehicles = await query.ToListAsync(_token);

      _logger.LogInformation("✅ {Count} adet müsait araç başarıyla getirildi.", vehicles.Count);

      // 8. Mapping (Veri belleğe alındığı için Domain metotları burada sorunsuz çalışır)
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
          v.GetMainImageUrl() ?? string.Empty, // Artık bellekte olduğu için hata vermez
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
      // Hatanın detayını görmek için loga yazdırıyoruz
      _logger.LogError(ex, "❌ Müsait araçlar getirilirken bir hata oluştu! Hata Detayı: {Message}", ex.Message);

      // Hata mesajını API'ye dönüyoruz ki ne olduğunu görebilelim
      return Result<PagedResponse<VehicleListDto>>.Failure($"Müsait araçlar getirilirken hata oluştu: {ex.Message} | Inner: {ex.InnerException?.Message}");
    }
  }
}