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

      var models = await query
          .Select(v => v.VehicleModel.Name)
          .Distinct()
          .OrderBy(m => m)
          .ToListAsync(_token);

      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);

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

/*
                        KULLANIM ALANLARI
      ---------------------------------------------------------------------
      1️⃣	Filtreleme Dropdown'u	      Kullanıcı modele göre filtreleyebilsin
      2️⃣	Arama Sayfası	              Gelişmiş arama filtresi
      3️⃣	Raporlama	                  Modele göre rapor filtreleme
      4️⃣	Admin Paneli	              Araç yönetiminde filtreleme
      5️⃣	Ana Sayfa	                  Popüler modelleri gösterme
*/