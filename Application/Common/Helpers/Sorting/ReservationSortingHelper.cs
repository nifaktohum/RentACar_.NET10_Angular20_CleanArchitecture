using Domain.Entities.Reservations;
using Domain.Specifications;

namespace Application.Common.Helpers.Sorting;

public static class ReservationSortingHelper
{
  public static void ApplyReservationSorting(this BaseSpecification<Reservation> spec, string? sort)
  {
    switch (sort)
    {
      case "startDateAsc":
        spec.ApplyOrderBy(r => r.PlannedStartDate);
        break;
      case "startDateDesc":
        spec.ApplyOrderByDescending(r => r.PlannedStartDate);
        break;
      case "endDateAsc":
        spec.ApplyOrderBy(r => r.PlannedEndDate);
        break;
      case "endDateDesc":
        spec.ApplyOrderByDescending(r => r.PlannedEndDate);
        break;
      case "priceAsc":
        spec.ApplyOrderBy(r => r.TotalPrice);
        break;
      case "priceDesc":
        spec.ApplyOrderByDescending(r => r.TotalPrice);
        break;
      case "createdAsc":
        spec.ApplyOrderBy(r => r.CreatedAt);
        break;
      case "createdDesc":
      case "latest":
        spec.ApplyOrderByDescending(r => r.CreatedAt);
        break;
      default:
        // Varsayılan: en yeni rezervasyonlar önce
        spec.ApplyOrderByDescending(r => r.CreatedAt);
        spec.ApplyThenBy(r => r.PlannedStartDate);
        break;
    }
  }
}
