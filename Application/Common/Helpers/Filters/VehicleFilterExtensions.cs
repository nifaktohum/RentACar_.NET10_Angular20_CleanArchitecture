using System.Linq.Expressions;
using Domain.Entities.Vehicles;
using Application.Specifications;
using LinqKit;

namespace Application.Common.Helpers.Filters;

public static class VehicleFilterExtensions
{
  public static Expression<Func<Vehicle, bool>> BuildFilter(VehicleSpecParams? specParams, bool includeAll = false)
  {
    // 1. Başlangıç Koşulu
    // Admin için: Silinmemiş her şey
    // Müşteri için: Silinmemiş, Aktif ve Müsait olanlar
    Expression<Func<Vehicle, bool>> filter = includeAll
        ? v => !v.IsDeleted
        : v => v.Status == VehicleStatus.Available && v.IsActive && !v.IsDeleted;

    if (specParams == null) return filter;

    // 2. Marka Filtresi
    if (specParams.Brands != null && specParams.Brands.Any())
    {
      var brands = specParams.Brands.Select(b => b.ToLower()).ToList();
      filter = filter.And(v => brands.Contains(v.Brand.ToLower()));
    }

    // 3. Renk Filtresi
    if (specParams.Colors != null && specParams.Colors.Any())
    {
      var colors = specParams.Colors.Select(c => c.ToLower()).ToList();
      filter = filter.And(v => colors.Contains(v.Color.ToLower()));
    }

    // 4. Model Filtresi
    if (specParams.Models != null && specParams.Models.Any())
    {
      var models = specParams.Models.Select(m => m.ToLower()).ToList();
      filter = filter.And(v => v.VehicleModel != null && models.Contains(v.VehicleModel.Name.ToLower()));
    }

    // 5. Yakıt Filtresi
    if (specParams.FuelTypes != null && specParams.FuelTypes.Any())
    {
      var fuelTypes = specParams.FuelTypes.Select(f => f.ToLower()).ToList();
      filter = filter.And(v => fuelTypes.Contains(v.FuelType.ToLower()));
    }

    // 6. Vites Filtresi
    if (specParams.Transmissions != null && specParams.Transmissions.Any())
    {
      var transmissions = specParams.Transmissions.Select(t => t.ToLower()).ToList();
      filter = filter.And(v => transmissions.Contains(v.Transmission.ToLower()));
    }

    // 7. Yıl Filtresi
    if (specParams.Years != null && specParams.Years.Any())
    {
      var years = specParams.Years.ToList();
      filter = filter.And(v => years.Contains(v.Year));
    }

    // 8. Araç Tipi Filtresi (VehicleTypeId)
    if (specParams.VehicleTypeIds != null && specParams.VehicleTypeIds.Any())
    {
      var typeIds = specParams.VehicleTypeIds;
      filter = filter.And(v => typeIds.Contains(v.VehicleModel.VehicleTypeId));
    }

    // 9. Araç Tipi İsim Filtresi (Types)
    if (specParams.Types != null && specParams.Types.Any())
    {
      var vehicleTypeNames = specParams.Types.Select(t => t.ToLower()).ToList();
      filter = filter.And(v => v.VehicleModel != null && v.VehicleModel.VehicleType != null && vehicleTypeNames.Contains(v.VehicleModel.VehicleType.Name.ToLower()));
    }

    // 11. Fiyat Aralığı
    if (specParams.MinPrice.HasValue)
      filter = filter.And(v => v.DailyPrice >= specParams.MinPrice.Value);

    if (specParams.MaxPrice.HasValue)
      filter = filter.And(v => v.DailyPrice <= specParams.MaxPrice.Value);

    // 12. Koltuk & Kapı
    if (specParams.SeatCounts.HasValue)
      filter = filter.And(v => v.SeatCount == specParams.SeatCounts.Value);

    if (specParams.DoorCounts.HasValue)
      filter = filter.And(v => v.DoorCount == specParams.DoorCounts.Value);

    // 13. Müsaitlik Filtresi
    if (specParams.IsAvailable.HasValue)
    {
      var isAvailable = specParams.IsAvailable.Value;
      filter = filter.And(v =>
          (v.Status == VehicleStatus.Available && v.IsActive && !v.IsDeleted) == isAvailable);
    }

    // 14. Aktiflik Filtresi
    if (specParams.IsActive.HasValue)
      filter = filter.And(v => v.IsActive == specParams.IsActive.Value);

    // 15. Stok Filtreleri (VehicleModel üzerinden)
    if (specParams.IsInStock.HasValue)
    {
      bool inStock = specParams.IsInStock.Value;
      filter = filter.And(v => v.VehicleModel.Vehicles.Any(veh => veh.Status == VehicleStatus.Available && veh.IsActive && !veh.IsDeleted) == inStock);
    }

    // 16. Genel Arama (SearchTerm)
    if (!string.IsNullOrWhiteSpace(specParams.SearchTerm))
    {
      var search = specParams.SearchTerm.ToLower();

      filter = filter.And(v =>
          (v.VehicleModel != null && v.VehicleModel.Brand.ToLower().Contains(search)) ||
          (v.VehicleModel != null && v.VehicleModel.Name.ToLower().Contains(search)) ||
          (v.VehicleModel != null && v.VehicleModel.VehicleType != null && v.VehicleModel.VehicleType.Name.ToLower().Contains(search)) ||
          v.Plate.ToLower().Contains(search) ||
          v.Color.ToLower().Contains(search) ||
          v.FuelType.ToLower().Contains(search) ||
          v.Transmission.ToLower().Contains(search) ||
          v.Year.Contains(search) ||
          (v.Description != null && v.Description.ToLower().Contains(search)) ||
          (v.VinNumber != null && v.VinNumber.ToLower().Contains(search)) // 🆕 VIN ile arama eklendi
      );
    }

    return filter;
  }
}