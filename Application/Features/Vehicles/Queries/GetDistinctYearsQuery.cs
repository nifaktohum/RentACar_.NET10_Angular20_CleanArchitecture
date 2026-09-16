using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetDistinctYearsQuery : IRequest<Result<List<int>>>;

public sealed class GetDistinctYearsQueryHandler(
                        IVehicleRepository _vehicleRepo,
                        ILogger<GetDistinctYearsQueryHandler> _logger
                    ) : IRequestHandler<GetDistinctYearsQuery, Result<List<int>>>
{
  public async Task<Result<List<int>>> Handle(GetDistinctYearsQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Benzersiz yıllar getiriliyor...");

      var spec = new DistinctVehicleSpecifications.Years();

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      var years = await query
          .Select(v => v.Year)
          .Distinct()
          .OrderByDescending(y => y)  // En yeni yıl önce  // String sıralama
          .Select(y => int.Parse(y))  // string -> int dönüşümü
          .ToListAsync(_token);

      _logger.LogInformation("✅ {Count} benzersiz yıl getirildi", years.Count);

      return Result<List<int>>.Succeed(years);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Benzersiz yıllar getirilirken hata oluştu");
      return Result<List<int>>.Failure(500, _ex.Message);
    }
  }
}

/*
                  NEREDE KULLANILIR?
    ----------------------------------------------------
    Alan	              Açıklama
    Araç Arama Sayfası	Yıl filtresi olarak
    Admin Paneli	      Araç listesini yıla göre filtreleme
    Raporlama	          Yıla göre rapor filtreleme
    Ana Sayfa	          "2024 Modelleri" gibi öne çıkarma
    Mobil Uygulama	    Arama filtreleri
*/