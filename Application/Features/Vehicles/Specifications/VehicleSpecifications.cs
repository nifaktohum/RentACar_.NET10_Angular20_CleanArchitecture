using System.Linq.Expressions;
using Application.Common.Helpers.Filters;
using Application.Common.Helpers.Sorting;
using Domain.Entities.Vehicles;
using Application.Specifications;
using Domain.Specifications;
using Application.Common.Extensions;
using LinqKit;

namespace Application.Features.Vehicles.Specifications;

// /====================/ SPECIFICATIONS /====================/ //



// MÜSAİT ARAÇLAR (Stokta olan ve silinmemiş) 
public sealed class GetAllVehiclesSpecification : BaseSpecification<Vehicle>
{
  // Dışarıdan kriter almıyoruz, müsaitlik kuralını kendimiz veriyoruz.

  // 1. TÜM MÜSAİT ARAÇLAR
public GetAllVehiclesSpecification(VehicleSpecParams? specParams)
         : base(VehicleFilterExtensions.BuildFilter(specParams, includeAll: true))// includeAll: true ==> Tüm araçları getir. musait olmayanlarda!
  {
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    this.ApplyDistinct();

    // Sort bilgisini de doğrudan specParams içinden alıyoruz!
    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  
  }
}

public sealed class AvailableVehiclesSpecification : BaseSpecification<Vehicle>
{
  // Dışarıdan kriter almıyoruz, müsaitlik kuralını kendimiz veriyoruz.

  // 1. TÜM MÜSAİT ARAÇLAR
  public AvailableVehiclesSpecification(VehicleSpecParams? specParams)
         : base(VehicleFilterExtensions.BuildFilter(specParams))
  {
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    this.ApplyDistinct();

    // Sort bilgisini de doğrudan specParams içinden alıyoruz!
    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }
}

// ✅ TÜM TİPLER
public sealed class GetAllVehicleTypesSpecification : BaseSpecification<VehicleType>
{
  public GetAllVehicleTypesSpecification(VehicleSpecParams? specParams)
       : base(BuildCriteria(specParams))
  {
    // Include'lar
    AddInclude(vt => vt.VehicleModels);

    ApplyThenBy(vt => vt.Name);
    // Sıralama
    this.ApplyVehicleTypeSorting(specParams?.Sort);
    // ⭐ Sayfalama - pageNumber ve pageSize gönder
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }

  private static Expression<Func<VehicleType, bool>> BuildCriteria(VehicleSpecParams? specParams)
  {
    Expression<Func<VehicleType, bool>> criteria = vt => !vt.IsDeleted;

    // ⭐ IsActive filtresi
    if (specParams?.IsActive.HasValue == true)
    {
      criteria = criteria.And(vt => vt.IsActive == specParams.IsActive.Value);
    }

    // Arama filtresi
    if (!string.IsNullOrWhiteSpace(specParams?.SearchTerm))
    {
      var search = specParams.SearchTerm.Trim();
      criteria = criteria.And(vt => vt.Name.Contains(search) ||
          (vt.Description != null && vt.Description.Contains(search)));
    }

    return criteria;
  }
}

// MARKAYA GÖRE MÜSAİT ARAÇLAR
public sealed class AvailableVehiclesByBrandSpecification : BaseSpecification<Vehicle>
{
  public AvailableVehiclesByBrandSpecification(string? brand, VehicleSpecParams? specParams)
      : base(BuildCombinedExpression(brand, specParams))
  {
    // Application Helpers'e taşıdık global çözüm için
    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }

  private static Expression<Func<Vehicle, bool>> BuildCombinedExpression(string? brand, VehicleSpecParams? specParams)
  {
    var baseFilter = VehicleFilterExtensions.BuildFilter(specParams);

    // Eğer dışarıdan gelen bir marka filtresi varsa bunu da LINQ Expression ağacına dahil ediyoruz
    if (!string.IsNullOrWhiteSpace(brand))
    {
      baseFilter = baseFilter.And(v => v.Brand.ToLower() == brand.ToLower());
    }

    return baseFilter;
  }
}

// FİYAT ARALIĞINA GÖRE MÜSAİT ARAÇLAR
public sealed class AvailableVehiclesByPriceRangeSpecification : BaseSpecification<Vehicle>
{
  public AvailableVehiclesByPriceRangeSpecification(VehicleSpecParams specParams)
     : base(v => v.IsAvailable
                  && v.IsActive
                  && !v.IsDeleted
                  && (!specParams.MinPrice.HasValue || v.DailyPrice >= specParams.MinPrice.Value)
                  && (!specParams.MaxPrice.HasValue || v.DailyPrice <= specParams.MaxPrice.Value))
  {
    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }
}

// MODELe GÖRE MÜSAİT ARAÇLAR
public sealed class AvailableVehicleByModelSpecification : BaseSpecification<Vehicle>
{
  public AvailableVehicleByModelSpecification(string? model, VehicleSpecParams? specParams)
            : base(BuildCombinedExpression(model, specParams))
  // : base(v => v.IsAvailable && v.IsActive && !v.IsDeleted && (string.IsNullOrWhiteSpace(model) || v.Model == model))
  {
    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }

  private static Expression<Func<Vehicle, bool>> BuildCombinedExpression(string? model, VehicleSpecParams? specParams)
  {
    // 1. Temel dinamik filtreleri (fiyat, yıl vb.) alıyoruz
    var baseFilter = VehicleFilterExtensions.BuildFilter(specParams);
    // 2. Her durumda geçerli olması gereken temel araç durum kurallarını ekliyoruz
    baseFilter = baseFilter.And(v => v.IsAvailable && v.IsActive && !v.IsDeleted);

    // Eğer dışarıdan gelen bir marka filtresi varsa bunu da LINQ Expression ağacına dahil ediyoruz
    if (!string.IsNullOrWhiteSpace(model))
    {
      baseFilter = baseFilter.And(v => v.Model.ToLower() == model.ToLower());
    }

    return baseFilter;
  }
}

// ARAÇ DETAY  
public sealed class VehicleByIdSpecification : BaseSpecification<Vehicle>
{
  public VehicleByIdSpecification(Guid id)
      : base(v => v.Id == id && v.IsActive && !v.IsDeleted)
  {
  }
}

// ADMİN PLAKA SORGULAMA
public sealed class VehicleByPlateSpecification : BaseSpecification<Vehicle>
{
  public VehicleByPlateSpecification(string plate)
      : base(v => v.Plate.ToLower() == plate.ToLower() && !v.IsDeleted)
  {
  }
}

public sealed class VehicleModelByStockSpecification : BaseSpecification<VehicleModel>
{
  public VehicleModelByStockSpecification(int? minStock, int? maxStock, string? sort)
      : base(vm => vm.IsActive
                   && !vm.IsDeleted
                   && (!minStock.HasValue || vm.Stock >= minStock.Value)
                   && (!maxStock.HasValue || vm.Stock <= maxStock.Value))
  {
    this.ApplyVehicleModelSorting(sort);
  }
}

public sealed class VehicleTypesWithModelsSpecification : BaseSpecification<VehicleType>
{
  public VehicleTypesWithModelsSpecification()
      : base(vt => !vt.IsDeleted)
  {
    AddInclude(vt => vt.VehicleModels.Where(vm => !vm.IsDeleted));
  }
}

public sealed class VehiclesByPriceRangeSpecification : BaseSpecification<Vehicle>
{
  public VehiclesByPriceRangeSpecification(VehicleSpecParams specParams)
      : base(BuildCriteria(specParams.MinPrice, specParams.MaxPrice))
  {
    // Include'ler
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType); // Veya ThenInclude yapına göre
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());

  }

  private static Expression<Func<Vehicle, bool>> BuildCriteria(decimal? minPrice, decimal? maxPrice)
  {
    // Temel şartımız her zaman geçerli
    Expression<Func<Vehicle, bool>> baseCriteria = v => !v.IsDeleted && v.IsActive;

    // Not: Expression birleştirme (Expression Combine) kütüphanen yoksa, 
    // en garanti yol koşulları tek bir expression içinde yazmaktır:

    return v => !v.IsDeleted
                && v.IsActive
                && (!minPrice.HasValue || v.DailyPrice >= minPrice.Value)
                && (!maxPrice.HasValue || v.DailyPrice <= maxPrice.Value);
  }
}

public sealed class VehiclesByModelSpecification : BaseSpecification<Vehicle>
{
  public VehiclesByModelSpecification(string? model, VehicleSpecParams? specParams)
      : base(BuildCriteria(model))
  {
    // Include'ler
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }

  private static Expression<Func<Vehicle, bool>> BuildCriteria(string? model)
  {
    bool hasModel = !string.IsNullOrWhiteSpace(model);
    string? lowerModel = model?.ToLower();

    return v => !v.IsDeleted
                && v.IsActive
                && (!hasModel || v.Model.ToLower() == lowerModel);
  }
}

public sealed class VehiclesByBrandSpecification : BaseSpecification<Vehicle>
{
  public VehiclesByBrandSpecification(string? brand, VehicleSpecParams? specParams)
      : base(BuildCriteria(brand))
  {
    // Include'ler
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }

  private static Expression<Func<Vehicle, bool>> BuildCriteria(string? brand)
  {
    bool hasBrand = !string.IsNullOrWhiteSpace(brand);
    string? lowerBrand = brand?.ToLower();

    return v => !v.IsDeleted
                && v.IsActive
                && (!hasBrand || v.Brand.ToLower() == lowerBrand);
  }
}

public sealed class VehiclesByTypeSpecification : BaseSpecification<Vehicle>
{
  public VehiclesByTypeSpecification(Guid vehicleTypeId, VehicleSpecParams? specParams)
      : base(BuildCriteria(vehicleTypeId, specParams))
  // : base(v => !v.IsDeleted && v.IsActive && v.VehicleModel.VehicleTypeId == vehicleTypeId)
  {
    // Include'ler
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }

  private static Expression<Func<Vehicle, bool>> BuildCriteria(Guid vehicleTypeId, VehicleSpecParams? specParams)
  {
    // Temel filtreleri (fiyat, yıl, çoklu arama vb.) alıyoruz
    var filter = VehicleFilterExtensions.BuildFilter(specParams);

    // Araç tipi filtresini güvenli bir şekilde ekliyoruz
    filter = filter.And(v => !v.IsDeleted && v.IsActive && v.VehicleModel.VehicleTypeId == vehicleTypeId);

    return filter;
  }
}

public sealed class VehiclesByFuelTypeSpecification : BaseSpecification<Vehicle>
{
  public VehiclesByFuelTypeSpecification(string? fuelType, VehicleSpecParams? specParams)
      : base(BuildCriteria(fuelType, specParams))
  {
    // Include'ler
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));


    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }

  private static Expression<Func<Vehicle, bool>> BuildCriteria(string? fuelType, VehicleSpecParams? specParams)
  {
    var filter = VehicleFilterExtensions.BuildFilter(specParams);

    bool hasFuelType = !string.IsNullOrWhiteSpace(fuelType);
    string? lowerFuelType = fuelType?.ToLower();

    return v => !v.IsDeleted
                && v.IsActive
                && (!hasFuelType || v.FuelType.ToLower() == lowerFuelType);
  }
}

public sealed class VehiclesByTransmissionSpecification : BaseSpecification<Vehicle>
{
  public VehiclesByTransmissionSpecification(string? transmission, VehicleSpecParams? specParams)
      : base(BuildCriteria(transmission))
  {
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }

  private static Expression<Func<Vehicle, bool>> BuildCriteria(string? transmission)
  {
    bool hasTransmission = !string.IsNullOrWhiteSpace(transmission);
    string? lowerTransmission = transmission?.ToLower();

    return v => !v.IsDeleted
                && v.IsActive
                && (!hasTransmission || v.Transmission.ToLower() == lowerTransmission);
  }
}

public sealed class FeaturedVehiclesSpecification : BaseSpecification<Vehicle>
{
  public FeaturedVehiclesSpecification(VehicleSpecParams? specParams)
      : base(v => !v.IsDeleted && v.IsActive && v.IsAvailable)
  {
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }
}

public sealed class LatestVehiclesSpecification : BaseSpecification<Vehicle>
{
  public LatestVehiclesSpecification(VehicleSpecParams? specParams)
      : base(v => !v.IsDeleted && v.IsActive)
  {
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }
}

public sealed class RecommendedVehiclesSpecification : BaseSpecification<Vehicle>
{
  public RecommendedVehiclesSpecification(VehicleSpecParams specParams)
      : base(v => !v.IsDeleted && v.IsActive && v.IsAvailable)
  {
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    // Eğer dışarıdan özel bir sıralama gelmediyse, önerilenler için varsayılan olarak 
    // en yenileri veya en popülerleri öne çıkarabiliriz. 
    // specParams?.Sort boş gelirse "latest" (CreatedAt DESC) uygulayalım:
    var sortQuery = string.IsNullOrWhiteSpace(specParams?.Sort) ? "latest" : specParams.Sort;

    this.ApplyVehicleSorting(sortQuery);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }
}

public sealed class AdminVehicleListSpecification : BaseSpecification<Vehicle>
{
  public AdminVehicleListSpecification(VehicleSpecParams? specParams)
      : base(VehicleFilterExtensions.BuildFilter(specParams, includeAll: true)) // Admin panelinde aktif olmayanlar da görünsün ama silinenler hariç
  {
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    // Sort bilgisini de doğrudan specParams içinden alıyoruz!
    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }
}

// Araç istatistikleri 
public sealed class VehicleStatisticsSpecification : BaseSpecification<Vehicle>
{
  public VehicleStatisticsSpecification()
      : base(v => !v.IsDeleted)
  {
  }
}

public sealed class VehiclesForReportSpecification : BaseSpecification<Vehicle>
{
  public VehiclesForReportSpecification(VehicleSpecParams? specParams)
                  : base(VehicleFilterExtensions.BuildFilter(specParams, includeAll: true)) 
                  // Tüm araçlar (IsAvailable filtreleme yok)
                  // : base(v => !v.IsDeleted
                  //             && (!isActive.HasValue || v.IsActive == isActive.Value)
                  //             && (string.IsNullOrWhiteSpace(fuelType) || v.FuelType.ToLower() == fuelType.ToLower())
                  //             && (string.IsNullOrWhiteSpace(transmission) || v.Transmission.ToLower() == transmission.ToLower())
                  //             && (!startDate.HasValue || v.CreatedAt >= startDate.Value)
                  //             && (!endDate.HasValue || v.CreatedAt <= endDate.Value))
  {
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    this.ApplyVehicleSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }
}

public sealed class VehiclesByYearSpecification : BaseSpecification<Vehicle>
{
  public VehiclesByYearSpecification(int? year)
      : base(v => !v.IsDeleted
                  && v.IsActive
                 && (!year.HasValue || v.Year == year.Value.ToString()))
  {
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    // ApplyOrderBy(v => v.DailyPrice);
  }
}

public sealed class VehiclesByColorSpecification : BaseSpecification<Vehicle>
{
  public VehiclesByColorSpecification(string? color)
      : base(BuildCriteria(color))
  {
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    // ApplyOrderBy(v => v.DailyPrice);
  }

  private static Expression<Func<Vehicle, bool>> BuildCriteria(string? color)
  {
    bool hasColor = !string.IsNullOrWhiteSpace(color);
    string? lowerColor = color?.ToLower();

    return v => !v.IsDeleted
                && v.IsActive
                && (!hasColor || v.Color.ToLower().Contains(lowerColor!));
  }
}

public sealed class VehiclesBySeatCountSpecification : BaseSpecification<Vehicle>
{
  public VehiclesBySeatCountSpecification(int? seatCount)
      : base(v => !v.IsDeleted
                  && v.IsActive
                  && (!seatCount.HasValue || v.SeatCount == seatCount.Value))
  {
    AddInclude(v => v.VehicleModel);
    AddInclude(v => v.VehicleModel.VehicleType);
    AddInclude(v => v.Images.Where(i => !i.IsDeleted));

    // ApplyOrderBy(v => v.DailyPrice);
  }
}



// =========/ Private /========= //

