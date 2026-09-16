using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetDistinctFuelTypesQuery : IRequest<Result<List<string>>>;

public sealed class GetDistinctFuelTypesQueryHandler(
                        IVehicleRepository _vehicleRepo,
                        ILogger<GetDistinctFuelTypesQueryHandler> _logger
                    ) : IRequestHandler<GetDistinctFuelTypesQuery, Result<List<string>>>
{
  public async Task<Result<List<string>>> Handle(GetDistinctFuelTypesQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Benzersiz yakıt tipleri getiriliyor...");

      var spec = new DistinctVehicleSpecifications.FuelTypes();

      var query = spec.Criteria != null
       ? _vehicleRepo.Where(spec.Criteria)
       : _vehicleRepo.GetAll();

      var fuelTypes = await query
         .Select(v => v.FuelType)
         .Distinct()
         .OrderBy(f => f)
         .ToListAsync(_token);

      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

      _logger.LogInformation("✅ {Count} benzersiz yakıt tipi getirildi", fuelTypes.Count);

      return Result<List<string>>.Succeed(fuelTypes);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Benzersiz yakıt tipleri getirilirken hata oluştu");
      return Result<List<string>>.Failure(500, _ex.Message);
    }
  }
}