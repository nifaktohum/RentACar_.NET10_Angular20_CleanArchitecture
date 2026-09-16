using Domain.Entities.Vehicles;
using Domain.Specifications;

namespace Application.Features.Vehicles.Specifications;

public class DistinctVehicleSpecifications
{
  // 1️⃣ Benzersiz Markalar
  public sealed class Brands : BaseSpecification<Vehicle>
  {
    public Brands()
        : base(v => !v.IsDeleted && v.IsActive)
    {
      this.ApplyDistinct();
      this.ApplyOrderBy(v => v.VehicleModel.Brand);
    }
  }

  // 2️⃣ Benzersiz Modeller
  public sealed class Models : BaseSpecification<Vehicle>
  {
    public Models()
        : base(v => !v.IsDeleted && v.IsActive)
    {
      this.ApplyDistinct();
      this.ApplyOrderBy(v => v.VehicleModel.Name);
    }
  }

  // 3️⃣ Benzersiz Yakıt Tipleri
  public sealed class FuelTypes : BaseSpecification<Vehicle>
  {
    public FuelTypes()
        : base(v => !v.IsDeleted && v.IsActive)
    {
      ApplyDistinct();
      ApplyOrderBy(v => v.FuelType);
    }
  }

  // 4️⃣ Benzersiz Vites Tipleri
  public sealed class Transmissions : BaseSpecification<Vehicle>
  {
    public Transmissions()
        : base(v => !v.IsDeleted && v.IsActive)
    {
      ApplyDistinct();
      ApplyOrderBy(v => v.Transmission);
    }
  }

  // 5️⃣ Benzersiz Renkler
  public sealed class Colors : BaseSpecification<Vehicle>
  {
    public Colors()
        : base(v => !v.IsDeleted && v.IsActive)
    {
      ApplyDistinct();
      ApplyOrderBy(v => v.Color);
    }
  }

  // 6️⃣ Benzersiz Yıllar
  public sealed class Years : BaseSpecification<Vehicle>
  {
    public Years()
        : base(v => !v.IsDeleted && v.IsActive)
    {
      ApplyDistinct();
      ApplyOrderByDescending(v => v.Year);
    }
  }

  // 7️⃣ Benzersiz Araç Tipleri
  public sealed class VehicleTypes : BaseSpecification<VehicleType>
  {
    public VehicleTypes()
        : base(vt => !vt.IsDeleted && vt.IsActive)
    {
      ApplyDistinct();
      ApplyOrderBy(vt => vt.Name);
    }
  }

  // 8️⃣ Benzersiz Koltuk Sayıları
  public sealed class SeatCounts : BaseSpecification<Vehicle>
  {
    public SeatCounts()
        : base(v => !v.IsDeleted && v.IsActive)
    {
      ApplyDistinct();
      ApplyOrderBy(v => v.SeatCount);
    }
  }

  // 9️⃣ Benzersiz Kapı Sayıları
  public sealed class DoorCounts : BaseSpecification<Vehicle>
  {
    public DoorCounts()
        : base(v => !v.IsDeleted && v.IsActive)
    {
      ApplyDistinct();
      ApplyOrderBy(v => v.DoorCount);
    }
  }
}

// =====/ KULLANIM /=====//

/*
      // Handler'da      
      var spec = new DistinctSpecifications.Brands();
      var vehicles = await _vehicleRepo.GetAsync(spec, cancellationToken);

      var brands = vehicles
          .Select(v => v.VehicleModel.Brand)
          .ToList();

      // Veya direkt olarak:
      var spec = new DistinctSpecifications.FuelTypes();
      var fuelTypes = await _vehicleRepo.GetAsync(spec, cancellationToken);
      var fuelTypeList = fuelTypes.Select(v => v.FuelType).ToList();
*/
