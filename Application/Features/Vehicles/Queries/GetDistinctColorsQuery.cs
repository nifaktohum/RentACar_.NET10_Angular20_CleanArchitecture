using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetDistinctColorsQuery : IRequest<Result<List<string>>>;

public sealed class GetDistinctColorsQueryHandler(
    IVehicleRepository _vehicleRepo,
    ILogger<GetDistinctColorsQueryHandler> _logger
) : IRequestHandler<GetDistinctColorsQuery, Result<List<string>>>
{
  public async Task<Result<List<string>>> Handle(GetDistinctColorsQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Benzersiz renkler getiriliyor...");

      var spec = new DistinctVehicleSpecifications.Colors();

      var query = spec.Criteria != null
          ? _vehicleRepo.Where(spec.Criteria)
          : _vehicleRepo.GetAll();

      // Renkleri veritabanından çek, tekilleştir ve sırala
      var colors = await query
          .Select(v => v.Color)
          .Distinct()
          .OrderBy(c => c)
          .ToListAsync(_token);

      // ❌ Gereksiz OrderBy bloğu silindi. Zaten yukarıda SQL seviyesinde sıralama yapıldı.

      _logger.LogInformation("✅ {Count} benzersiz renk getirildi", colors.Count);

      return Result<List<string>>.Succeed(colors);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Benzersiz renkler getirilirken hata oluştu");
      return Result<List<string>>.Failure(500, _ex.Message);
    }
  }
}