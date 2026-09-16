using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetDistinctVehicleTypesQuery : IRequest<Result<List<string>>>;

public sealed class GetDistinctVehicleTypesQueryHandler(
                      IVehicleTypeRepository _vehicleTypeRepo,
                      ILogger<GetDistinctVehicleTypesQueryHandler> _logger
                  ) : IRequestHandler<GetDistinctVehicleTypesQuery, Result<List<string>>>
{
  public async Task<Result<List<string>>> Handle(GetDistinctVehicleTypesQuery _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🔍 Benzersiz araç tipleri getiriliyor...");

      var spec = new DistinctVehicleSpecifications.VehicleTypes();

      var query = spec.Criteria != null
          ? _vehicleTypeRepo.Where(spec.Criteria)
          : _vehicleTypeRepo.GetAll();

      var vehicleTypes = await query
          .Select(vt => vt.Name)
          .Distinct()
          .OrderBy(vt => vt)
          .ToListAsync(_token);

      _logger.LogInformation("✅ {Count} benzersiz araç tipi getirildi", vehicleTypes.Count);

      return Result<List<string>>.Succeed(vehicleTypes);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Benzersiz araç tipleri getirilirken hata oluştu");
      return Result<List<string>>.Failure(500, _ex.Message);
    }
  }
}

/*
    {
        "data": [
            "Convertible",
            "Coupe",
            "Hatchback",
            "Luxury",
            "Minivan",
            "Pickup",
            "Sedan",
            "StationWagon",
            "SUV",
            "Van"
        ],
        "errorMessages": null,
        "isSuccessful": true,
        "statusCode": 200
    }

                     KULLANIM ALANLARI
    ------------------------------------------------------------------
      Filtreleme Dropdown'u	            Kullanıcı araç tipine göre filtreleyebilsin
      Arama Sayfası	                    Gelişmiş arama filtresi
      Raporlama	                        Araç tipine göre rapor filtreleme
      Admin Paneli	                    Araç yönetiminde filtreleme
      Ana Sayfa	                        Popüler araç tiplerini gösterme
*/