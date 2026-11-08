using Domain.Entities.Vehicles;
using Domain.Specifications;

namespace Application.Common.Helpers.Sorting;

public static class VehicleModelSortingHelper
{
  public static void ApplyVehicleModelSorting(this BaseSpecification<VehicleModel> spec, string? sort)
  {
    if (string.IsNullOrWhiteSpace(sort))
    {
      // Varsayılan: Marka + Model adı (artan)
      spec.ApplyOrderBy(vm => vm.Brand);
      spec.ApplyThenBy(vm => vm.Name);
      return;
    }

    switch (sort.ToLower())
    {
      // Stok miktarına göre sıralama (Computed Property hatası çözüldü)
      case "stockasc":
        spec.ApplyOrderBy(vm => vm.Vehicles.Count(v => v.IsActive && !v.IsDeleted));
        spec.ApplyThenBy(vm => vm.Brand);
        break;
      case "stockdesc":
        spec.ApplyOrderByDescending(vm => vm.Vehicles.Count(v => v.IsActive && !v.IsDeleted));
        spec.ApplyThenBy(vm => vm.Brand);
        break;

      // Müsait stok miktarına göre sıralama (Computed Property hatası çözüldü)
      case "availableasc":
        spec.ApplyOrderBy(vm => vm.Vehicles.Count(v => v.IsAvailable));
        spec.ApplyThenBy(vm => vm.Brand);
        break;
      case "availabledesc":
        spec.ApplyOrderByDescending(vm => vm.Vehicles.Count(v => v.IsAvailable));
        spec.ApplyThenBy(vm => vm.Brand);
        break;

      // İsme göre sıralama
      case "nameasc":
        spec.ApplyOrderBy(vm => vm.Name);
        spec.ApplyThenBy(vm => vm.Brand);
        break;
      case "namedesc":
        spec.ApplyOrderByDescending(vm => vm.Name);
        spec.ApplyThenBy(vm => vm.Brand);
        break;

      // Markaya göre sıralama
      case "brandasc":
        spec.ApplyOrderBy(vm => vm.Brand);
        spec.ApplyThenBy(vm => vm.Name);
        break;
      case "branddesc":
        spec.ApplyOrderByDescending(vm => vm.Brand);
        spec.ApplyThenBy(vm => vm.Name);
        break;

      // Tarihe göre sıralama
      case "createdasc":
      case "oldest":
        spec.ApplyOrderBy(vm => vm.CreatedAt);
        spec.ApplyThenBy(vm => vm.Name);
        break;
      case "createddesc":
      case "latest":
        spec.ApplyOrderByDescending(vm => vm.CreatedAt);
        spec.ApplyThenBy(vm => vm.Name);
        break;

      default:
        // Varsayılan: Marka + Model adı (artan)
        spec.ApplyOrderBy(vm => vm.Brand);
        spec.ApplyThenBy(vm => vm.Name);
        break;
    }
  }
}