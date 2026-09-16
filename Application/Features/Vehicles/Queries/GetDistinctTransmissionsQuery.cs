using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetDistinctTransmissionsQuery : IRequest<Result<List<string>>>;

public sealed class GetDistinctTransmissionsQueryHandler(
                        IVehicleRepository _vehicleRepo,
                        ILogger<GetDistinctTransmissionsQueryHandler> _logger
                    ) : IRequestHandler<GetDistinctTransmissionsQuery, Result<List<string>>>
{
  public async Task<Result<List<string>>> Handle(GetDistinctTransmissionsQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Benzersiz vites tipleri getiriliyor...");

      var spec = new DistinctVehicleSpecifications.Transmissions();

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      var transmissions = await query
          .Select(v => v.Transmission)
          .Distinct()
          .OrderBy(t => t)
          .ToListAsync(_token);

      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

      _logger.LogInformation("✅ {Count} benzersiz vites tipi getirildi", transmissions.Count);

      return Result<List<string>>.Succeed(transmissions);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Benzersiz vites tipleri getirilirken hata oluştu");
      return Result<List<string>>.Failure(500, _ex.Message);
    }
  }
}