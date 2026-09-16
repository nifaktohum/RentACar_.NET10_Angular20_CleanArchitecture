

using Application.Common.Extensions;
using Application.Common.Models;
using Application.Features.VehicleModels.Dto;
using Application.Features.Vehicles.Specifications;
using Application.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.VehicleModels.Queries;

public sealed record GetAllVehicleModelsQuery : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleModelListDto>>>;

public sealed class GetAllVehicleModelsQueryHandler(
                        IVehicleModelRepository _vehicleModelRepo
                    ) : IRequestHandler<GetAllVehicleModelsQuery, Result<PagedResponse<VehicleModelListDto>>>
{
  public async Task<Result<PagedResponse<VehicleModelListDto>>> Handle(GetAllVehicleModelsQuery _req, CancellationToken _token)
  {



    // ✅ 5. Sıralama - OrderBy + ThenBy
    // 1. Specification oluştur
    var spec = new GetAllVehicleModelSpecifications(_req);

    // 2. Query oluştur
    var query = spec.Criteria != null
        ? _vehicleModelRepo.Where(spec.Criteria)
        : _vehicleModelRepo.GetAll();

    // ✅ 3. Include'leri uygula!
    query = query.ApplyIncludes(spec);

    var totalCount = await query.CountAsync(_token);

    // 4. ✅ Sıralama - OrderBy + ThenBy
    query = query.ApplyOrdering(spec);

    // 5. ✅ Sayfalama
    if (spec.IsPagingEnabled)
    {
      query = query.Skip(spec.Skip).Take(spec.Take);
    }


    // 7. Verileri çek
    var vehicleModels = await query.ToListAsync(_token);

    // 8. DTO'ya map et
    var items = vehicleModels.Select(x => new VehicleModelListDto(
        x.Id,
        x.Brand,
        x.Name,
        x.Description,
        x.Stock,
        x.AvailableStock,
        x.IsInStock,
        x.IsActive,
        x.VehicleTypeId,
        x.VehicleType?.Name ?? "Belirtilmemiş",
        x.CreatedAt
    )).ToList();

    // 9. PagedResponse oluştur
    var response = new PagedResponse<VehicleModelListDto>(
          items,
          totalCount,        // ✅ 2. parametre: TotalCount
          _req.PageNumber,   // ✅ 3. parametre: PageNumber
          _req.PageSize      // ✅ 4. parametre: PageSize
      );

    return Result<PagedResponse<VehicleModelListDto>>.Succeed(response);
  }
}