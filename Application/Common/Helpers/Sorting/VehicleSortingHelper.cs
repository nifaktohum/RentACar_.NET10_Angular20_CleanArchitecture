using Domain.Entities.Vehicles;
using Domain.Specifications;

namespace Application.Common.Helpers.Sorting;

public static class VehicleSortingHelper
{
  // BaseSpecification<Vehicle> sınıfına extension (uzantı) yazıyoruz
  public static void ApplyVehicleSorting(this BaseSpecification<Vehicle> spec, string? sort)
  {

    // Reflection veya dinamik tip casting derdi olmadan, doğrudan Vehicle bildiğimiz için güvenle yazıyoruz:
    switch (sort)
    {
      // Price
      case "dailyPriceAsc":
        spec.ApplyOrderBy(v => v.DailyPrice);
        break;
      case "dailyPriceDesc":
        spec.ApplyOrderByDescending(v => v.DailyPrice);
        break;
      // Year
      case "yearAsc":
        spec.ApplyOrderBy(v => v.Year);
        break;
      case "yearDesc":
        spec.ApplyOrderByDescending(v => v.Year);
        break;
      // Brand
      case "brandAsc":
        spec.ApplyOrderBy(v => v.Brand);
        break;
      case "brandDesc":
        spec.ApplyOrderByDescending(v => v.Brand);
        break;
      // Model
      case "modelAsc":
        spec.ApplyOrderBy(v => v.Model);
        break;
      case "modelDesc":
        spec.ApplyOrderByDescending(v => v.Model);
        break;
      // Create
      case "createdAsc":
        spec.ApplyOrderBy(v => v.CreatedAt);
        break;
      case "createdDesc":
      case "latest":
        spec.ApplyOrderByDescending(v => v.CreatedAt);
        break;

      default:
        // Varsayılan olarak fiyata göre artan sırala
        spec.ApplyOrderBy(v => v.DailyPrice);
        // spec.ApplyOrderByDescending(v => v.CreatedAt);
        spec.ApplyThenBy(v => v.Brand);
        break;
    }
  }
}
