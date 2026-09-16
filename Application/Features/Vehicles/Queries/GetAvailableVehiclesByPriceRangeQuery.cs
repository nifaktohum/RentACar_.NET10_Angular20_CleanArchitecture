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

public sealed record GetAvailableVehiclesByPriceRangeQuery : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetAvailableVehiclesByPriceRangeQueryHandler(
                        IVehicleRepository _vehicleRepo,
                        ILogger<GetAvailableVehiclesByPriceRangeQueryHandler> _logger
                    ) : IRequestHandler<GetAvailableVehiclesByPriceRangeQuery, Result<PagedResponse<VehicleListDto>>>
{
  public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetAvailableVehiclesByPriceRangeQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("Müsait araçlar fiyat aralığına göre sorgulanıyor. Min: {MinPrice} ₺, Max: {MaxPrice} ₺", _req.MinPrice, _req.MaxPrice);

      // Kriterimizi query'den gelen fiyat aralığı ile oluşturuyoruz.
      var spec = new AvailableVehiclesByPriceRangeSpecification(_req);

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      // 3. Include'ları EKLE! (ÇOK ÖNEMLİ)
      query = query
          .Include(v => v.VehicleModel).ThenInclude(vm => vm.VehicleType)
          .Include(v => v.Images.Where(i => i.IsActive && !i.IsDeleted))
          .OrderBy(v => v.DailyPrice); // Fiyata göre sırala (en ucuzdan en pahalıya)

      // Specification içinde belirlenen sıralama kuralını sorguya uyguluyoruz
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

      // 4. Veritabanına sorguyu gönder
      var totalCount = await query.CountAsync(_token);

      if (spec.IsPagingEnabled)
        query = query.Skip(spec.Skip).Take(spec.Take);

      var vehicles = await query.ToListAsync(_token);

      // 2. Türkçe Bilgi Logu (Başarılı Sonuç)
      _logger.LogInformation("{Count} adet müsait araç başarıyla getirildi. Fiyat Aralığı: {MinPrice} ₺ - {MaxPrice} ₺", vehicles.Count, _req.MinPrice, _req.MaxPrice);

      // Entity -> DTO Dönüşümü
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
          v.GetMainImageUrl() ?? string.Empty,    // Null safety
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
      // 3. Türkçe Hata Logu
      _logger.LogError(ex, "Araçlar fiyat aralığına göre getirilirken sistemde bir hata oluştu! Min: {MinPrice}, Max: {MaxPrice}", _req.MinPrice, _req.MaxPrice);

      return Result<PagedResponse<VehicleListDto>>.Failure("Araçlar fiyat aralığına göre getirilirken bir hata oluştu.");
    }
  }
}