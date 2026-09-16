using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetDistinctSeatCountsQuery : IRequest<Result<List<int>>>;

public sealed class GetDistinctSeatCountsQueryHandler(
                        IVehicleRepository _vehicleRepo,
                        ILogger<GetDistinctSeatCountsQueryHandler> _logger
                    ) : IRequestHandler<GetDistinctSeatCountsQuery, Result<List<int>>>
{
  public async Task<Result<List<int>>> Handle(GetDistinctSeatCountsQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Benzersiz koltuk sayıları getiriliyor...");

      var spec = new DistinctVehicleSpecifications.SeatCounts();

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      var seatCounts = await query
          .Select(v => v.SeatCount)
          .Distinct()
          .OrderBy(s => s)
          .ToListAsync(_token);

      _logger.LogInformation("✅ {Count} benzersiz koltuk sayısı getirildi", seatCounts.Count);

      return Result<List<int>>.Succeed(seatCounts);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Benzersiz koltuk sayıları getirilirken hata oluştu");
      return Result<List<int>>.Failure(500, _ex.Message);
    }
  }
}

/*
      {
        "data": [2, 4, 5, 6, 7, 8, 9],
        "errorMessages": null,
        "isSuccessful": true,
        "statusCode": 200
      }

                KULLANIM ALANLARI
    -----------------------------------------------------   
    Filtreleme Dropdown'u	      Kullanıcı koltuk sayısına göre filtreleyebilsin
    Arama Sayfası	              Gelişmiş arama filtresi
    Raporlama	                  Koltuk sayısına göre rapor filtreleme
    Admin Paneli	              Araç yönetiminde filtreleme


*/