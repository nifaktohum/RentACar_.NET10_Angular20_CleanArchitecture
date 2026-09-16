using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Vehicles;
using Domain.Specifications;

namespace Application.Common.Helpers.Sorting;

public static class VehicleTypeSortingHelper
{
  public static void ApplyVehicleTypeSorting(this BaseSpecification<VehicleType> spec, string? sort)
  {
    if (string.IsNullOrWhiteSpace(sort))
    {
      // Varsayılan: DisplayOrder + Name (artan)
      spec.ApplyOrderBy(vt => vt.DisplayOrder);
      spec.ApplyThenBy(vt => vt.Name);
      return;
    }

    switch (sort.ToLower())
    {
      // İsme göre sıralama
      case "nameasc":
        spec.ApplyOrderBy(vt => vt.Name);
        spec.ApplyThenBy(vt => vt.DisplayOrder);
        break;
      case "namedesc":
        spec.ApplyOrderByDescending(vt => vt.Name);
        spec.ApplyThenBy(vt => vt.DisplayOrder);
        break;

      // DisplayOrder'a göre sıralama
      case "orderasc":
        spec.ApplyOrderBy(vt => vt.DisplayOrder);
        spec.ApplyThenBy(vt => vt.Name);
        break;
      case "orderdesc":
        spec.ApplyOrderByDescending(vt => vt.DisplayOrder);
        spec.ApplyThenBy(vt => vt.Name);
        break;

      // Model sayısına göre sıralama (sadece Include yapıldıysa çalışır)
      case "modelcountasc":
        spec.ApplyOrderBy(vt => vt.VehicleModels.Count(vm => !vm.IsDeleted));
        spec.ApplyThenBy(vt => vt.Name);
        break;
      case "modelcountdesc":
        spec.ApplyOrderByDescending(vt => vt.VehicleModels.Count(vm => !vm.IsDeleted));
        spec.ApplyThenBy(vt => vt.Name);
        break;

      // Aktiflik durumuna göre sıralama
      case "activefirst":
        spec.ApplyOrderByDescending(vt => vt.IsActive);
        spec.ApplyThenBy(vt => vt.DisplayOrder);
        break;
      case "activefirstbyname":
        spec.ApplyOrderByDescending(vt => vt.IsActive);
        spec.ApplyThenBy(vt => vt.Name);
        break;

      // Tarihe göre sıralama
      case "createdasc":
      case "oldest":
        spec.ApplyOrderBy(vt => vt.CreatedAt);
        spec.ApplyThenBy(vt => vt.Name);
        break;
      case "createddesc":
      case "latest":
        spec.ApplyOrderByDescending(vt => vt.CreatedAt);
        spec.ApplyThenBy(vt => vt.Name);
        break;


      default:
        // Varsayılan: DisplayOrder + Name (artan)
        spec.ApplyOrderBy(vt => vt.DisplayOrder);
        spec.ApplyThenBy(vt => vt.Name);
        break;
    }
  }
}
