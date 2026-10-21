using System.Linq.Expressions;
using Application.Specifications;
using Domain.Entities.Reservations;
using LinqKit;

namespace Application.Common.Helpers.Filters;

public static class ReservationFilterExtensions
{
  public static Expression<Func<Reservation, bool>> BuildFilter(ReservationSpecParams? specParams, bool includeAll = false)
  {
    // Başlangıç koşulu: Silinmemiş rezervasyonlar
    // includeAll true ise (Admin) → IsActive filtresi yok
    // includeAll false ise (Müşteri) → sadece aktif olanlar
    Expression<Func<Reservation, bool>> filter = includeAll
        ? r => !r.IsDeleted
        : r => !r.IsDeleted && r.IsActive;

    if (specParams == null) return filter;

    // Müşteri filtresi
    if (specParams.CustomerId.HasValue)
      filter = filter.And(r => r.CustomerId == specParams.CustomerId.Value);

    // Araç filtresi
    if (specParams.VehicleId.HasValue)
      filter = filter.And(r => r.VehicleId == specParams.VehicleId.Value);

    // Şube filtresi
    if (specParams.PickupBranchId.HasValue)
      filter = filter.And(r => r.PickupBranchId == specParams.PickupBranchId.Value);

    // Durum filtresi
    if (specParams.Status.HasValue)
      filter = filter.And(r => r.Status == specParams.Status.Value);

    // Tarih filtreleri (Planlanan başlangıç/bitiş)
    if (specParams.StartDate.HasValue)
      filter = filter.And(r => r.PlannedStartDate >= specParams.StartDate.Value);

    if (specParams.EndDate.HasValue)
      filter = filter.And(r => r.PlannedEndDate <= specParams.EndDate.Value);

    // Aktiflik filtresi (override)
    if (specParams.IsActive.HasValue)
      filter = filter.And(r => r.IsActive == specParams.IsActive.Value);

    // Rezervasyon numarası (tam eşleşme)
    if (!string.IsNullOrWhiteSpace(specParams.ReservationNumber))
    {
      var number = specParams.ReservationNumber.ToLower();
      filter = filter.And(r => r.ReservationNumber.ToLower() == number);
    }

    // Genel arama
    if (!string.IsNullOrWhiteSpace(specParams.SearchTerm))
    {
      var search = specParams.SearchTerm.ToLower();
      filter = filter.And(r =>
          r.ReservationNumber.ToLower().Contains(search) ||
          (r.Customer.FirstName != null && r.Customer.FirstName.ToLower().Contains(search)) ||
          (r.Customer.LastName != null && r.Customer.LastName.ToLower().Contains(search)) ||
          (r.Customer.CompanyName != null && r.Customer.CompanyName.ToLower().Contains(search)) ||
          (r.Customer.PhoneNumber != null && r.Customer.PhoneNumber.Contains(search))
      );
    }

    return filter;
  }
}
