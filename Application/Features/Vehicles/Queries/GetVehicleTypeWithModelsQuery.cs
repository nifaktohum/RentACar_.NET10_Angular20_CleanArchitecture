using Application.Features.VehicleModels.Dto;
using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetVehicleTypeWithModelsQuery : IRequest<Result<List<VehicleTypeWithModelsDto>>>;

public sealed class GetVehicleTypeWithModelsQueryHandler(
    IVehicleTypeRepository _vehicleTypeRepo,
    ILogger<GetVehicleTypeWithModelsQueryHandler> _logger
) : IRequestHandler<GetVehicleTypeWithModelsQuery, Result<List<VehicleTypeWithModelsDto>>>
{
  public async Task<Result<List<VehicleTypeWithModelsDto>>> Handle(GetVehicleTypeWithModelsQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("Tüm araç tipleri ve bağlı modelleri getiriliyor...");


      var spec = new VehicleTypesWithModelsSpecification();

      var query = spec.Criteria != null
       ? _vehicleTypeRepo.Where(spec.Criteria)
       : _vehicleTypeRepo.GetAll();

      foreach (var include in spec.Includes)
      {
        query = query.Include(include);
      }

      var result = query.Select(vt => new VehicleTypeWithModelsDto(
          vt.Id,
          vt.Name,
          vt.Description,
          vt.Icon,
          vt.DisplayOrder,
          vt.VehicleModels.Count(vm => vm.IsActive && !vm.IsDeleted),
          vt.VehicleModels.Select(vm => new VehicleModelSummaryDto(
              vm.Id,
              vm.Brand,
              vm.Name,
              vm.Stock,
              vm.AvailableStock
          )).ToList(),
          vt.IsActive,
          vt.CreatedAt
      )).ToList();

      _logger.LogInformation("✅ {Count} adet araç tipi ve modelleri başarıyla getirildi", result.Count);

      return Result<List<VehicleTypeWithModelsDto>>.Succeed(result);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Araç tipleri ve modelleri getirilirken hata oluştu");
      return Result<List<VehicleTypeWithModelsDto>>.Failure(500, $"Araç tipleri getirilirken hata oluştu: {_ex.Message}");
    }
  }
}