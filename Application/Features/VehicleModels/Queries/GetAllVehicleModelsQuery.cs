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
    // 1. Specification oluştur
    var spec = new GetAllVehicleModelSpecifications(_req);

    // 2. Query oluştur
    var query = spec.Criteria != null
        ? _vehicleModelRepo.Where(spec.Criteria)
        : _vehicleModelRepo.GetAll();

    // 3. Include'leri uygula (Extension metot)
    query = query.ApplyIncludes(spec);

    // Toplam sayı
    var totalCount = await query.CountAsync(_token);

    // 4. Sıralama (Extension metot)
    query = query.ApplyOrdering(spec);

    // 5. Sayfalama
    if (spec.IsPagingEnabled)
    {
      query = query.Skip(spec.Skip).Take(spec.Take);
    }

    // 6. Verileri çek ve DTO'ya Map et (Projection kullanarak Stock hatasını çözüyoruz)
    var items = await query.Select(x => new VehicleModelListDto(
        x.Id,
        x.Brand,
        x.Name,
        x.Description,
        x.Vehicles.Count(v => v.IsActive && !v.IsDeleted), // ✅ Doğru Stock Hesabı
        x.Vehicles.Count(v => v.IsAvailable),              // ✅ Doğru AvailableStock Hesabı
        x.Vehicles.Any(v => v.IsAvailable),                // ✅ IsInStock (En az 1 tane müsait var mı?)
        x.IsActive,
        x.VehicleTypeId,
        x.VehicleType != null ? x.VehicleType.Name : "Belirtilmemiş", // Null safety
        x.CreatedAt
    )).ToListAsync(_token); // Asenkron çekiyoruz

    // 7. PagedResponse oluştur
    var response = new PagedResponse<VehicleModelListDto>(
        items,
        totalCount,
        _req.PageNumber,
        _req.PageSize
    );

    return Result<PagedResponse<VehicleModelListDto>>.Succeed(response);
  }
}