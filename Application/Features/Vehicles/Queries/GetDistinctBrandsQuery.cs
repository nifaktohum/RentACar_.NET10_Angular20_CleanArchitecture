using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed class GetDistinctBrandsQuery : IRequest<Result<List<string>>>;


public sealed class GetDistinctBrandsQueryHandler(
                      IVehicleRepository _vehicleRepo,
                      ILogger<GetDistinctBrandsQueryHandler> _logger
                    ) : IRequestHandler<GetDistinctBrandsQuery, Result<List<string>>>
{
  public async Task<Result<List<string>>> Handle(GetDistinctBrandsQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Benzersiz markalar getiriliyor...");

      var spec = new DistinctVehicleSpecifications.Brands();
      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      var brands = await query
             .Select(v => v.VehicleModel.Brand)
             .Distinct()
             .OrderBy(b => b)
             .ToListAsync(_token);

      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);
      

      _logger.LogInformation("✅ {Count} benzersiz marka getirildi", brands.Count);

      return Result<List<string>>.Succeed(brands);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Benzersiz markalar getirilirken hata oluştu");
      return Result<List<string>>.Failure(500, _ex.Message);
    }
  }
}

