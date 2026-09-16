using Application.Common.Models;
using Application.Features.Vehicles.Dto;
using Application.Features.Vehicles.Specifications;
using Application.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleTypes.Queries;

public sealed record GetAllVehicleTypesQuery : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleTypeDto>>>;

public sealed class GetAllVehicleTypesQueryHandler(
                          IVehicleTypeRepository _vehicleTypeRepo,
                          ILogger<GetAllVehicleTypesQuery> _logger
                    ) : IRequestHandler<GetAllVehicleTypesQuery, Result<PagedResponse<VehicleTypeDto>>>
{
  public async Task<Result<PagedResponse<VehicleTypeDto>>> Handle(GetAllVehicleTypesQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Araç tipleri getiriliyor...");

      // 1. Specification oluştur
      var spec = new GetAllVehicleTypesSpecification(_req);

      // ⭐ 2. Query oluştur (Vehicle'deki gibi)
      var query = spec.Criteria != null
         ? _vehicleTypeRepo.Where(spec.Criteria)
         : _vehicleTypeRepo.GetAll();

      // ⭐ 3. Include'ları uygula
      foreach (var include in spec.Includes)
      {
        query = query.Include(include);
      }

      // ⭐ 4. Sıralama
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

      // ⭐ 5. Toplam sayı (Count)
      var totalCount = await query.CountAsync(_token);
      _logger.LogInformation("📊 Toplam kayıt: {TotalCount}", totalCount);

      // ⭐ 6. Sayfalama (Paging)
      if (spec.IsPagingEnabled)
        query = query.Skip(spec.Skip).Take(spec.Take);

      // ⭐ 7. Verileri çek
      var vehicleTypes = await query.ToListAsync(_token);

      // ⭐ 8. DTO'ya dönüştür
      var result = vehicleTypes.Select(v => new VehicleTypeDto(
          v.Id,
          v.Name,
          v.Description,
          v.Icon,
          v.DisplayOrder,
          v.VehicleModels.Count(vm => !vm.IsDeleted),
          v.IsActive,
          v.CreatedAt
      )).ToList();

      _logger.LogInformation("✅ {Count} araç tipi getirildi. Toplam: {TotalCount}",
          result.Count, totalCount);

      // ⭐ 9. PagedResponse oluştur
      var response = new PagedResponse<VehicleTypeDto>(
          result,
          totalCount,
          _req.PageNumber,
          _req.PageSize
      );

      return Result<PagedResponse<VehicleTypeDto>>.Succeed(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Araç tipleri getirilirken hata oluştu");
      return Result<PagedResponse<VehicleTypeDto>>.Failure(
          500,
          $"Araç tipleri getirilirken hata oluştu: {ex.Message}"
      );
    }


  }
}