using Domain.Entities.Reservations;
using Domain.Specifications;

namespace Application.Common.Helpers.Sorting;

public static class ReservationSortingHelper
{
  public static void ApplyReservationSorting(this BaseSpecification<Reservation> spec, string? sort)
  {
    switch (sort)
    {
      case "reservationnumberasc":
        spec.ApplyOrderBy(r => r.ReservationNumber);
        break;
      case "reservationnumberdesc":
        spec.ApplyOrderByDescending(r => r.ReservationNumber);
        break;
      case "customerfullnameasc":
        spec.ApplyOrderBy(r => r.Customer);
        break;
      case "customerfullnamedesc":
        spec.ApplyOrderByDescending(r => r.Customer);
        break;
      case "plannedstartdateasc":
        spec.ApplyOrderBy(r => r.PlannedStartDate);
        break;
      case "plannedstartdatedesc":
        spec.ApplyOrderByDescending(r => r.PlannedStartDate);
        break;
      case "enddateasc":
        spec.ApplyOrderBy(r => r.PlannedEndDate);
        break;
      case "enddatedesc":
        spec.ApplyOrderByDescending(r => r.PlannedEndDate);
        break;
      case "totalpriceasc":
        spec.ApplyOrderBy(r => r.TotalPrice);
        break;
      case "totalpricedesc":
        spec.ApplyOrderByDescending(r => r.TotalPrice);
        break;
      case "createdasc":
        spec.ApplyOrderBy(r => r.CreatedAt);
        break;
      case "createddesc":
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
