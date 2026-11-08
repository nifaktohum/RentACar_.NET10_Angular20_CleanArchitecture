using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetDistinctModelsQuery : IRequest<Result<List<string>>>;

public sealed class GetDistinctModelsQueryHandler(
    IVehicleRepository _vehicleRepo,
    ILogger<GetDistinctModelsQueryHandler> _logger
) : IRequestHandler<GetDistinctModelsQuery, Result<List<string>>>
{
  public async Task<Result<List<string>>> Handle(GetDistinctModelsQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Benzersiz modeller getiriliyor...");

      var spec = new DistinctVehicleSpecifications.Models();

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      // Modelleri veritabanından çek, tekilleştir ve sırala
      var models = await query
          .Select(v => v.VehicleModel.Name)
          .Distinct()
          .OrderBy(m => m)
          .ToListAsync(_token);

      // ❌ Gereksiz OrderBy bloğu silindi. SQL seviyesinde OrderBy(m => m) zaten yapıldı.

      _logger.LogInformation("✅ {Count} benzersiz model getirildi", models.Count);

      return Result<List<string>>.Succeed(models);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Benzersiz modeller getirilirken hata oluştu");
      return Result<List<string>>.Failure(500, _ex.Message);
    }
  }
}