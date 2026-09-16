using System.Linq.Expressions;
using Domain.Entities.Vehicles;
using Application.Specifications;
using LinqKit;

namespace Application.Common.Helpers.Filters;

public static class VehicleFilterExtensions
{
  public static Expression<Func<Vehicle, bool>> BuildFilter(VehicleSpecParams? specParams, bool includeAll = false)
  {
    // Başlangıç koşulu: Tüm aktif ve müsait araçlar
    // Expression<Func<Vehicle, bool>> filter = v => v.IsAvailable && v.IsActive && !v.IsDeleted;
    // ⭐ includeAll true ise IsAvailable filtreleme, false ise sadece müsait araçlar
    Expression<Func<Vehicle, bool>> filter = includeAll
                                      ? v => !v.IsDeleted                                    // Admin: Sadece silinmemişler (Aktif/Pasif fark etmez)
                                      : v => v.IsAvailable && v.IsActive && !v.IsDeleted;    // Müşteri: Müsait, aktif ve silinmemişler
    if (specParams == null) return filter;

    // Marka filtresi varsa ekle
    if (specParams.Brands != null && specParams.Brands.Any())
    {
      var brands = specParams.Brands.Select(b => b.ToLower()).ToList();
      filter = filter.And(v => brands.Contains(v.Brand.ToLower()));
      //.And ==>  İki Expression<Func<T, bool>> ifadesini güvenli bir şekilde && ile birleştirmek için ya LinqKit kütüphanesini kullanman gerekir
    }

    // Renk filtresi varsa ekle
    if (specParams.Colors != null && specParams.Colors.Any())
    {
      var colors = specParams.Colors.Select(c => c.ToLower()).ToList();
      filter = filter.And(v => colors.Contains(v.Color.ToLower()));
    }

    // Model filtresi
    if (specParams.Models != null && specParams.Models.Any())
    {
      var models = specParams.Models.Select(m => m.ToLower()).ToList();
      filter = filter.And(v => models.Contains(v.VehicleModel.Name.ToLower()));
    }

    // Yakıt filtresi
    if (specParams.FuelTypes != null && specParams.FuelTypes.Any())
    {
      var fuelTypes = specParams.FuelTypes.Select(f => f.ToLower()).ToList();
      filter = filter.And(v => fuelTypes.Contains(v.FuelType.ToLower()));
    }

    // Vites filtresi
    if (specParams.Transmissions != null && specParams.Transmissions.Any())
    {
      var transmissions = specParams.Transmissions.Select(t => t.ToLower()).ToList();
      filter = filter.And(v => transmissions.Contains(v.Transmission.ToLower()));
    }

    // Yıl filtresi
    if (specParams.Years != null && specParams.Years.Any())
    {
      var years = specParams.Years.ToList();
      filter = filter.And(v => years.Contains(v.Year));
    }

    // TARİH FİLTRELERİ
    if (specParams.StartDate.HasValue)
      filter = filter.And(v => v.CreatedAt >= specParams.StartDate.Value);

    if (specParams.EndDate.HasValue)
      filter = filter.And(v => v.CreatedAt <= specParams.EndDate.Value);

    // Fiyat aralığı
    if (specParams.MinPrice.HasValue)
      filter = filter.And(v => v.DailyPrice >= specParams.MinPrice.Value);

    if (specParams.MaxPrice.HasValue)
      filter = filter.And(v => v.DailyPrice <= specParams.MaxPrice.Value);

    // Koltuk
    if (specParams.SeatCounts.HasValue)
      filter = filter.And(v => v.SeatCount == specParams.SeatCounts.Value);

    // Kapı
    if (specParams.DoorCounts.HasValue)
      filter = filter.And(v => v.DoorCount == specParams.DoorCounts.Value);

    // IsAvailable (Müsaitlik) filtresi varsa ekle
    if (specParams.IsAvailable.HasValue)
    {
      filter = filter.And(v => v.IsAvailable == specParams.IsAvailable.Value);
    }

    if (specParams.Types != null && specParams.Types.Any() )
    {
      var vehicleTypeName = specParams.Types.ToList();
      filter = filter.And(v => vehicleTypeName.Contains(v.VehicleModel.VehicleType.Name));
    }

    // tekli Arama Filtresi 
    if (!string.IsNullOrWhiteSpace(specParams.SearchTerm))
    {
      var search = specParams.SearchTerm.ToLower();

      filter = filter.And(v =>
              v.VehicleModel.Brand.ToLower().Contains(search) ||          // Marka
              v.VehicleModel.Name.ToLower().Contains(search) ||          // Model
              v.VehicleModel.VehicleType.Name.ToLower().Contains(search) ||           // Araç Tipi
              v.Plate.ToLower().Contains(search) ||                      // Plaka
              v.Color.ToLower().Contains(search) ||                      // Renk
              v.FuelType.ToLower().Contains(search) ||                   // Yakıt
              v.Transmission.ToLower().Contains(search) ||               // Vites
              v.Year.Contains(search) ||                                 // Yıl
              (v.Description != null && v.Description.ToLower().Contains(search)) // Açıklama
          );
      
    }

    return filter;
  }
}
