using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetDistinctDoorCountsQuery : IRequest<Result<List<int>>>;

public sealed class GetDistinctDoorCountsQueryHandler(
                        IVehicleRepository _vehicleRepo,
                        ILogger<GetDistinctDoorCountsQueryHandler> _logger
                    ) : IRequestHandler<GetDistinctDoorCountsQuery, Result<List<int>>>
{
  public async Task<Result<List<int>>> Handle(GetDistinctDoorCountsQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Benzersiz kapı sayıları getiriliyor...");

      var spec = new DistinctVehicleSpecifications.DoorCounts();

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      var doorCounts = await query
          .Select(v => v.DoorCount)
          .Distinct()
          .OrderBy(d => d)
          .ToListAsync(_token);

      _logger.LogInformation("✅ {Count} benzersiz kapı sayısı getirildi", doorCounts.Count);

      return Result<List<int>>.Succeed(doorCounts);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Benzersiz kapı sayıları getirilirken hata oluştu");
      return Result<List<int>>.Failure(500, _ex.Message);
    }
  }
}

/*
  {
    "data": [2, 3, 4, 5],
    "errorMessages": null,
    "isSuccessful": true,
    "statusCode": 200
  }

                KULLANIM ALANLARI
----------------------------------------------------------  
Filtreleme Dropdown'u	              Kullanıcı kapı sayısına göre filtreleyebilsin
Arama Sayfası	                      Gelişmiş arama filtresi
Raporlama	                          Kapı sayısına göre rapor filtreleme
Admin Paneli	                      Araç yönetiminde filtreleme
*/