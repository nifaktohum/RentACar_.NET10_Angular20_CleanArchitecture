using Domain.Entities.Vehicles;
using Domain.Specifications;

namespace Application.Common.Helpers.Sorting;

public static class VehicleSortingHelper
{
  public static void ApplyVehicleSorting(this BaseSpecification<Vehicle> spec, string? sort)
  {
    // 🆕 Gelen sıralama parametresini küçük harfe çeviriyoruz (Null-safe olarak)
    var normalizedSort = sort?.ToLowerInvariant();

    switch (normalizedSort)
    {
      // Price
      case "dailypriceasc":
        spec.ApplyOrderBy(v => v.DailyPrice);
        break;
      case "dailypricedesc":
        spec.ApplyOrderByDescending(v => v.DailyPrice);
        break;
      // Year
      case "yearasc":
        spec.ApplyOrderBy(v => v.Year);
        break;
      case "yeardesc":
        spec.ApplyOrderByDescending(v => v.Year);
        break;
      // Brand
      case "brandasc":
        spec.ApplyOrderBy(v => v.Brand);
        break;
      case "branddesc":
        spec.ApplyOrderByDescending(v => v.Brand);
        break;
      // Model
      case "modelasc":
        spec.ApplyOrderBy(v => v.Model);
        break;
      case "modeldesc":
        spec.ApplyOrderByDescending(v => v.Model);
        break;
      // Create
      case "createdasc":
        spec.ApplyOrderBy(v => v.CreatedAt);
        break;
      case "createddesc":
      case "latest":
        spec.ApplyOrderByDescending(v => v.CreatedAt);
        break;

      default:
        // Varsayılan olarak fiyata göre artan sırala
        spec.ApplyOrderBy(v => v.DailyPrice);
        spec.ApplyThenBy(v => v.Brand);
        break;
    }
  }
}