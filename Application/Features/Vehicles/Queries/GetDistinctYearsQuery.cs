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

      // 1. Önce veritabanından sadece benzersiz string yılları çekiyoruz (SQL'e çevrilebilir)
      var distinctYearsAsString = await query
          .Select(v => v.Year)
          .Distinct()
          .ToListAsync(_token);

      // 2. C# tarafında int'e çevir, int olarak sırala ve listele (En yeni yıl en üstte)
      var years = distinctYearsAsString
          .Where(y => int.TryParse(y, out _)) // 🆕 Güvenlik: Sadece geçerli sayıları al
          .Select(y => int.Parse(y))
          .OrderByDescending(y => y)
          .ToList();

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