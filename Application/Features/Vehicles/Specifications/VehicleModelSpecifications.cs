using Application.Common.Extensions;
using Application.Common.Helpers.Sorting;
using Application.Specifications;
using Domain.Entities.Vehicles;
using Domain.Specifications;
using LinqKit;

namespace Application.Features.Vehicles.Specifications;

public sealed class GetAllVehicleModelSpecifications : BaseSpecification<VehicleModel>
{
  public GetAllVehicleModelSpecifications(VehicleSpecParams? specParams)
          : base(BuildCriteria(specParams))
  {
    AddInclude(vm => vm.VehicleType);

    // Gelişmiş sıralama yardımcısını çağırıyoruz
    this.ApplyVehicleModelSorting(specParams?.Sort);

    // Sayfalama
    this.ApplyPaging(specParams ?? new VehicleSpecParams());
  }


  private static System.Linq.Expressions.Expression<Func<VehicleModel, bool>> BuildCriteria(VehicleSpecParams? specParams)
  {
    var filter = PredicateBuilder.New<VehicleModel>(vm => !vm.IsDeleted);

    if (specParams == null)
      return filter;

    // Brand (Marka) filtresi çoklu destek
    if (specParams.Brands is { Count: > 0 })
    {
      filter = filter.And(vm => specParams.Brands.Any(b => vm.Brand.ToLower().Contains(b.ToLower())));
    }

    // Model (İsim) filtresi çoklu destek
    if (specParams.Models is { Count: > 0 })
    {
      filter = filter.And(vm => specParams.Models.Any(m => vm.Name.ToLower().Contains(m.ToLower())));
    }

    // VehicleTypeId filtresi - çoklu destek
    if (specParams.VehicleTypeIds is { Count: > 0 })
    {
      filter = filter.And(vm => specParams.VehicleTypeIds.Contains(vm.VehicleTypeId));
    }

    // Stok filtresi
    if (specParams.IsInStock.HasValue)
    {
      if (specParams.IsInStock.Value)
        filter = filter.And(vm => vm.Vehicles.Any(v =>
            v.Status == VehicleStatus.Available && v.IsActive && !v.IsDeleted));
      else
        filter = filter.And(vm => !vm.Vehicles.Any(v =>
            v.Status == VehicleStatus.Available && v.IsActive && !v.IsDeleted));
    }

    // Minimum stok filtresi
    if (specParams.MinStock.HasValue)
    {
      var min = specParams.MinStock.Value;
      filter = filter.And(vm => vm.Vehicles.Count(v => v.IsActive && !v.IsDeleted) >= min);
    }

    // Maksimum stok filtresi
    if (specParams.MaxStock.HasValue)
    {
      var max = specParams.MaxStock.Value;
      filter = filter.And(vm => vm.Vehicles.Count(v => v.IsActive && !v.IsDeleted) <= max);
    }

    // Genel arama (SearchTerm)
    if (!string.IsNullOrWhiteSpace(specParams.SearchTerm))
    {
      var term = specParams.SearchTerm.ToLower();
      filter = filter.And(vm => vm.Brand.ToLower().Contains(term) || vm.Name.ToLower().Contains(term));
    }

    return filter;
  }
}

public sealed class GetVehicleModelByIdSpecification : BaseSpecification<VehicleModel>
{
  public GetVehicleModelByIdSpecification(Guid id)
      : base(vm => vm.Id == id && !vm.IsDeleted && vm.IsActive)
  {
    // Include'ler
    AddInclude(vm => vm.VehicleType);
  }
}
