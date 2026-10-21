using Application.Common.Extensions;
using Application.Common.Helpers.Filters;
using Application.Common.Helpers.Sorting;
using Application.Specifications;
using Domain.Entities.Reservations;
using Domain.Specifications;

namespace Application.Features.Reservations.Specifications;


// ==================== DETAY ====================

public sealed class ReservationByIdWithDetailsSpecification : BaseSpecification<Reservation>
{
  public ReservationByIdWithDetailsSpecification(Guid id)
      : base(r => r.Id == id && !r.IsDeleted)
  {
    AddInclude(r => r.Customer);
    AddInclude(r => r.Vehicle);
    AddInclude(r => r.Vehicle.VehicleModel);
    AddInclude(r => r.PickupBranch);
    AddInclude(r => r.ReturnBranch!);
    AddInclude(r => r.Extras);
    // AddInclude(r => r.Extras.Select(e => e.Extra));
    AddInclude("Extras.Extra");
  }
}

public sealed class ReservationByIdSpecification : BaseSpecification<Reservation>
{
  public ReservationByIdSpecification(Guid id)
      : base(r => r.Id == id && !r.IsDeleted)
  {
  }
}

// ==================== LİSTE ====================

public sealed class GetAllReservationsSpecification : BaseSpecification<Reservation>
{
  public GetAllReservationsSpecification(ReservationSpecParams? specParams, bool includeAll = false)
      : base(ReservationFilterExtensions.BuildFilter(specParams, includeAll))
  {
    AddInclude(r => r.Customer);
    AddInclude(r => r.Vehicle);
    AddInclude(r => r.Vehicle.VehicleModel);
    AddInclude(r => r.PickupBranch);

    this.ApplyReservationSorting(specParams?.Sort);
    this.ApplyPaging(specParams ?? new ReservationSpecParams());
  }
}

// Müşterinin rezervasyonları
public sealed class ReservationsByCustomerSpecification : BaseSpecification<Reservation>
{
  public ReservationsByCustomerSpecification(Guid customerId, ReservationSpecParams? specParams = null)
      : base(r => r.CustomerId == customerId && !r.IsDeleted)
  {
    AddInclude(r => r.Vehicle);
    AddInclude(r => r.Vehicle.VehicleModel);
    AddInclude(r => r.PickupBranch);

    this.ApplyReservationSorting(specParams?.Sort ?? "latest");
    this.ApplyPaging(specParams ?? new ReservationSpecParams());
  }
}

// ==================== MÜSAİTLİK ====================

public sealed class ConflictingReservationsSpecification : BaseSpecification<Reservation>
{
  public ConflictingReservationsSpecification(Guid vehicleId, DateTimeOffset startDate, DateTimeOffset endDate)
      : base(r => r.VehicleId == vehicleId
          && !r.IsDeleted
          && (r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Active)
          && r.PlannedStartDate < endDate
          && startDate < r.PlannedEndDate)
  {
  }
}

// ==================== JOB ====================

public sealed class ExpiredPendingPaymentsSpecification : BaseSpecification<Reservation>
{
  public ExpiredPendingPaymentsSpecification()
      : base(r => r.Status == ReservationStatus.PendingPayment
          && r.PaymentDeadline < DateTimeOffset.UtcNow
          && !r.IsDeleted)
  {
    AddInclude(r => r.Vehicle);
  }
}

public sealed class ExpiredConfirmedPickupsSpecification : BaseSpecification<Reservation>
{
  public ExpiredConfirmedPickupsSpecification()
      : base(r => r.Status == ReservationStatus.Confirmed
          && r.PickupDeadline.HasValue
          && r.PickupDeadline.Value < DateTimeOffset.UtcNow
          && !r.IsDeleted)
  {
    AddInclude(r => r.Vehicle);
  }
}
